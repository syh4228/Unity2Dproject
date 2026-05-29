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

    // 총 발사 함수
    public void FireCurrentGun(bool isLookLeft)
    {
        // 내가 사용중에 있는 총기가 있고, 그게 1번 총기 인지 2번총기 인지 확인해서 저장
        WeaponData currentGun = (activeGunIndex == 1) ? UseGun1 : UseGun2;
        // 내가 사용중인 총기가 1번 총기의 탄창인지, 2번 총기의 탄창인지 확인해서 저장
        ref int currentMag = ref (activeGunIndex == 1) ? ref gun1Magazine : ref gun2Magazine;
        // 내가 사용중인 총기가 1번 총기의 예비 탄창인지, 2번 총기의 예비 탄창인지 확인해서 저장
        ref int currentRes = ref (activeGunIndex == 1) ? ref gun1Reserve : ref gun2Reserve;

        // 총이 없으면 반환
        if (currentGun == null) return;

        // 탄창에 총알이 있으면
        if (currentMag > 0)
        {
            currentMag--; // 총알 -1
            // 웨폰매니저 총알발사함수 호출
            WeaponManager.FireBullet(isLookLeft, currentGun.Damage);

            //  탄창과 예비 탄창 둘다 0 이면
            if (currentMag == 0 && currentRes == 0)
            {
                // 들고 있는 총이 있으면
                if (activeGunIndex == 1)
                {
                    // 1번 총기 버리기
                    UseGun1 = null;
                    // 2번에 총이 있다면 2번으로 스왑
                    if (UseGun2 != null) activeGunIndex = 2;
                }
                else
                {   // 2번 슬롯 총기 버리기
                    UseGun2 = null;
                    // 1번에 총이 있다면 1번으로 스왑
                    if (UseGun1 != null) activeGunIndex = 1;
                }

                UtillLogRemove.Log("총알이 다 떨어져서 무기를 버렸습니다!");
                UpdateItemUI(); // UI도 갱신!
            }
        }
    }

    // 재장전 함수
    public void ReloadCurrentGun()
    {
        WeaponData currentGun = (activeGunIndex == 1) ? UseGun1 : UseGun2;
        ref int magazine = ref (activeGunIndex == 1) ? ref gun1Magazine : ref gun2Magazine;
        ref int reserve = ref (activeGunIndex == 1) ? ref gun1Reserve : ref gun2Reserve;

        if (currentGun == null) return;

        // 채워야 할 총알 개수 계산 (총기 최대 용량 - 현재 탄창)
        int need = currentGun.Capacity - magazine;

        // 채워야 할 총알이 있고, 예비 탄창에 총알이 남아있다면
        if (need > 0 && reserve > 0)
        {
            // 채울 총알 갯수와, 예비탄창 총알 갯수 저장
            int reloadAmount = Mathf.Min(need, reserve);

            magazine += reloadAmount; // 탄창에 총알 채우기
            reserve -= reloadAmount; // 예비 탄창에서 채운 총알 만큼 빼기

            // UI매니저에 총알갯수업데이트 UI 함수 호출
            UiManager.UpdateAmmoUI(activeGunIndex, magazine, reserve);
            UtillLogRemove.Log("재장전 완료!");
        }

        if (UseGun1 == null && UseGun2 == null)
        {
            WeaponData ptGunData = GameDataManager.Instance.GetWeaponData("Weapon_PT_1");

            if (ptGunData != null)
            {
                UseGun1 = ptGunData; // 1번 슬롯에 기본 총 할당
                gun1Magazine = ptGunData.Capacity; // 탄창 가득 채우기
                gun1Reserve = ptGunData.Capacity2; // 예비 탄알 채우기
                activeGunIndex = 1; // 1번 총을 들고 있게 함

                UtillLogRemove.Log("무기가 없습니다! 기본 무기(PT)를 장착합니다.");
                UpdateItemUI(); // UI 갱신
            }
        }
    }

    // 총기 스압 함수
    public void ChangeActiveGun(int slotIndex)
    {
        // 1번 슬롯을 눌렀고, 총이 없다면 반환
        if (slotIndex == 1 && UseGun1 == null) return;

        // 2번 슬롯을 눌렀고, 총이 없다면 반환
        if (slotIndex == 2 && UseGun2 == null) return;

        // 현재 총기 슬롯 번호에 저장
        activeGunIndex = slotIndex;

        UtillLogRemove.Log($"{activeGunIndex}번 총기로 교체 완료!");
    }

    // 폭탄 
    public void UseBoomItem()
    {
        // 폭탄 없으면 반환
        if (UseBoom == null) return;

        UtillLogRemove.Log("폭탄 투척!");

        // 폭탄 삭제
        UseBoom = null;

        // UI아이템 업데이트 함수 호출
        UpdateItemUI();
    }
}
