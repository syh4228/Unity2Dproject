using UnityEngine;
using System.Collections.Generic;

public class Player_InventoryManager : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private WeaponManager WeaponManager; // 무기 매니저
    [SerializeField] private BattleUIManager UiManager; // 배틀 UI 매니저
    [SerializeField] private Player_Character PlayerCharacter; // 플레이어 캐릭터 매니저

    private WeaponData UseGun1; // 소지하고 있는 1번 총기
    private int gun1Magazine; // 1번 사용 총기 한 탄창 총알
    private int gun1Reserve; // 1번 사용 총기 총 탄창 총알

    private WeaponData UseGun2; // 2번 총기
    private int gun2Magazine; 
    private int gun2Reserve;

    private int activeGunIndex = 1; // 현재 사용상태 총기

    private WeaponData UseBoom;  // 수류탄
    private WeaponData UseHeel1; // 메디킷
    private WeaponData UseHeel2; // 진통제 / 아드레날린

    // 획득 아이템 관리 함수
    public void PickUpItem(string itemId)
    {
        // 데이터 매니저에서 주운 아이템 정보 가져오기
        var itemData = GameDataManager.Instance.GetWeaponData(itemId);

        if (itemData != null)
        {
            return;
        }

        string useType = itemData.UseType; // 타입 저장
        string id = itemData.Id; // 아이디 저장

        if (useType == "Gun")
        {
            EquipGun(itemData);
        }
        else if (useType == "Boom")
        {
            if (UseBoom == null)
            {
                UseBoom = itemData;
                UpdateItemUI();
            }
            else UtillLogRemove.Log("이미 폭탄을 가지고 있습니다!");
        }
        else if (useType == "Heel")
        {
            EquipHeel(itemData, id);
        }
    }

    // 총기 주울떄 함수
    private void EquipGun(WeaponData newGun)
    {
        if (activeGunIndex == 1)
        {
            // 1번 총을 들고 있을 때 주우면 1번 슬롯 교체
            SetGunSlot(1, newGun);
        }
        else if (activeGunIndex == 2)
        {
            // 2번 총을 들고 있을 때, 1번이 비어있다면 1번에 채우고 1번으로 자동 스왑
            if (UseGun1 == null)
            {
                SetGunSlot(1, newGun);
                activeGunIndex = 1;
            }
            else
            {
                // 1번이 차있는데 2번을 들고 주웠다면, 2번 슬롯 교체
                SetGunSlot(2, newGun);
            }
        }
    }

    // 가지고 있는 총기 슬롯 함수
    private void SetGunSlot(int slot, WeaponData gunData)
    {
        if (slot == 1)
        {
            UseGun1 = gunData;
            gun1Magazine = gunData.Capacity;
            gun1Reserve = gunData.Capacity2;
        }
        else
        {
            UseGun2 = gunData;
            gun2Magazine = gunData.Capacity;
            gun2Reserve = gunData.Capacity2;
        }
    }

    // 회복 아이템 줍기 함수
    private void EquipHeel(WeaponData itemData, string id)
    {
        if (id.Contains("HK")) // 메디킷
        {
            if (UseHeel1 == null)
            {
                UseHeel1 = itemData;
                UpdateItemUI();
            }
            else Debug.Log("이미 구급상자가 있습니다!");
        }
        else if (id.Contains("MD") || id.Contains("AD")) // 진통제, 아드레날린
        {
            // 같은 종류를 이미 들고 있으면 습득 불가
            if (UseHeel2 != null && UseHeel2.Id == id)
            {
                Debug.Log($"이미 {itemData.Name}을(를) 들고 있습니다!");
                return;
            }

            // 비어있거나, MD인데 AD를 줍는 등 다른 종류면 교체 가능
            UseHeel2 = itemData;
            UpdateItemUI();
        }
    }

    //  힐킷 사용 함수
    public void UseHeelItem1()
    {
        if (UseHeel1 == null) return;

        // PlayerCharacter 스크립트로 연산 넘기기
        PlayerCharacter.ApplyHeal_Hk();

        UseHeel1 = null; // 사용 후 슬롯 비우기
        UpdateItemUI();
    }

    // 아드, 구급약 사용 함수
    public void UseHeelItem2()
    {
        if (UseHeel2 == null) return;

        if (UseHeel2.Id.Contains("MD")) PlayerCharacter.ApplyHeal_MD();
        else if (UseHeel2.Id.Contains("AD")) PlayerCharacter.ApplyHeal_AD();

        UseHeel2 = null; // 사용 후 슬롯 비우기
        UpdateItemUI();
    }

    // 아이템 UI 업데이트
    private void UpdateItemUI()
    {
        UiManager.UpdateItemUI(UseBoom != null, UseHeel1 != null, UseHeel2 != null);
    }


    public void FireCurrentGun(bool isLookLeft)
    {
        WeaponData currentGun = (activeGunIndex == 1) ? UseGun1 : UseGun2;
        ref int currentMag = ref (activeGunIndex == 1) ? ref gun1Magazine : ref gun2Magazine;
        ref int currentRes = ref (activeGunIndex == 1) ? ref gun1Reserve : ref gun2Reserve;

        if (currentGun == null) return;

        if (currentMag > 0)
        {
            currentMag--;
            WeaponManager.FireBullet(isLookLeft, currentGun.Damage);

            // 만약 총알을 다 썼는데, 예비 탄약(Reserve)도 0이라면 총기 슬롯을 비움
            if (currentMag == 0 && currentRes == 0)
            {
                if (activeGunIndex == 1)
                {
                    UseGun1 = null; // 1번 총기 버림
                    if (UseGun2 != null) activeGunIndex = 2; // 2번 총이 있으면 2번으로 자동 스왑
                }
            }
        }
    }
}
