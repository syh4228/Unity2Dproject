using UnityEngine;
using System.Collections.Generic;

public class Player_InventoryManager : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private WeaponManager WeaponManager; // 무기 매니저
    [SerializeField] private BattleUIManager UiManager; // 배틀 UI 매니저
    [SerializeField] private Player_Character PlayerCharacter; // 플레이어 캐릭터 매니저

    [Header("수류탄 컴포넌트")]
    [SerializeField] private GameObject grenedPrefab; // 수류탄 프리팹
    [SerializeField] private Transform thowPoint; // 수류탄 던질때 생성될 위치

    [SerializeField] private AnimationController AnimController; // 애니메이션 컨트롤러 연결

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

    private void Start()
    {
        // 게임데이터 매니저에서 기본 권총 1의 정보를 가져와 저장
        WeaponData ptGunData = GameDataManager.Instance.GetWeaponData("Weapon_PT_1");

        if (ptGunData != null ) // 권총이 있으면
        {
            UseGun1 = ptGunData; // 1번 무기 슬롯에 권총 장착
            gun1Magazine = ptGunData.Capacity; // 탄창 채우기
            gun1Reserve = ptGunData.Capacity2; // 예비 탄창 채우기
            activeGunIndex = 1; // 무기 1번으로 설정

            UtillLogRemove.Log("기본권총 들고 시작");
            UpdateItemUI(); // UI 갱신
        }
        else
        {
            UtillLogRemove.Error("기본권총 데이터 없어요.");
        }
    }

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

    // 총기 주울때 함수
    private void EquipGun(WeaponData newGun)
    {
        // 1번 무기를 들고 있을 때 주우면
        if (activeGunIndex == 1)
        {
            // 2번 슬롯이 비어있는지 확인
            if (UseGun2 == null)
            {
                // 비어있다면 새 무기를 2번에 넣기
                SetGunSlot(2, newGun);
            }
            else // 2번이 차있으면
            {
                // 현재 들고 있는 1번 무기를 버리고 새 무기로 교체
                SetGunSlot(1, newGun);
            }
        }
        // 2번 무기를 들고 있을 때 주우면
        else if (activeGunIndex == 2)
        {
            // 1번 슬롯이 비어있는지 확인
            if (UseGun1 == null)
            {
                // 비어있다면 새 무기를 1번에 넣기
                SetGunSlot(1, newGun);
            }
            else // 1번이 차있다면
            {
                // 현재 들고 있는 2번 무기를 버리고 새 무기로 교체
                SetGunSlot(2, newGun);
            }
        }

        UpdateItemUI(); // UI업데이트
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
        // 아이템이 전부 있을때, UI 업데이트
        UiManager.UpdateItemUI(UseBoom != null, UseHeel1 != null, UseHeel2 != null);
        
        if (UseGun1 != null) // 건슬롯 1번이 있으면
        {
            // 배틀UI매니저에 업데이트 웨폰 슬롯 UI 함수 호출
            UiManager.UpdateWeaponSlotUI(1, UseGun1.IconPath, activeGunIndex == 1);
            // 배틀UI매니저에 업데이트 총알 UI 함수 호출
            UiManager.UpdateAmmoUI(1, gun1Magazine, gun1Reserve);
        }
        else // 없으면
        {
            // 배틀 UI 매니저에 없다고 알림
            UiManager.UpdateWeaponSlotUI(1, null, false);
            // 배틀 UI 매니저에 없다고 알림
            UiManager.UpdateAmmoUI(1, 0, 0);
        }

        if (UseGun2 != null) // 건 슬롯 2번이 있으면
        {
            UiManager.UpdateWeaponSlotUI(2, UseGun2.IconPath, activeGunIndex == 2);
            UiManager.UpdateAmmoUI(2, gun2Magazine, gun2Reserve);
        }
        else
        {
            UiManager.UpdateWeaponSlotUI(2, null, false);
            UiManager.UpdateAmmoUI(2, 0, 0);
        }

        // 현재 들고 있는 총이 1번 슬롯에 있는 총이면 1번 저장, 2번 슬롯이면 2번 저장
        WeaponData currentGun = (activeGunIndex == 1) ? UseGun1 : UseGun2;

        // 총이 있고, 애니메이터 컨트롤러가 연결되어 있다면
        if (currentGun != null && AnimController != null)
        {
            // 애니메이터에게 JSON에 적힌 모션으로 바꾸라고 지시
            AnimController.ChangeWeaponAnimation(currentGun.Anim_AttackPath, currentGun.Anim_ReloadPath);
            // 애니메이터에게 JSON에 적힌 RPM 속도로 쏘라고 지시
            AnimController.SetAttackSpeed(currentGun.RPM);
        }
    }

    // 총 발사 함수
    public bool TryFireCurrentGun(bool isLookLeft)
    {
        // 내가 사용중에 있는 총기가 있고, 그게 1번 총기 인지 2번총기 인지 확인해서 저장
        WeaponData currentGun = (activeGunIndex == 1) ? UseGun1 : UseGun2;
        // 내가 사용중인 총기가 1번 총기의 탄창인지, 2번 총기의 탄창인지 확인해서 저장
        ref int currentMag = ref (activeGunIndex == 1) ? ref gun1Magazine : ref gun2Magazine;
        // 내가 사용중인 총기가 1번 총기의 예비 탄창인지, 2번 총기의 예비 탄창인지 확인해서 저장
        ref int currentRes = ref (activeGunIndex == 1) ? ref gun1Reserve : ref gun2Reserve;

        // 발사 방향, 총 대미지, 대미지 타입, 사거리, 사속 웨폰메니저에서 받아서 저장
        bool isFired = WeaponManager.FireBullet(isLookLeft, currentGun);

        if (isFired == true) // 발사가 트루면
        {
            currentMag--; // 총알 - 1

            // UI 매니저에 총알 소모 알림
            UiManager.UpdateAmmoUI(activeGunIndex, currentMag, currentRes);

            // 탄창이 0 이고, 예비 총알이 0이면
            if (currentMag == 0 && currentRes == 0)
            {
                // 1번 총기면
                if (activeGunIndex == 1)
                {
                    UseGun1 = null; // 1번 총기 슬롯 비우기
                                    // 만약 2번 총기 슬롯에 총이 있으면 2번총기로 바꾸기
                    if (UseGun2 != null) activeGunIndex = 2;
                }
                else // 2번 총기면
                {
                    UseGun2 = null; // 2번 총기 슬롯 비우기
                                    // 만약 1번 총기 슬롯에 총이 있으면 1번 총기로 바꾸기
                    if (UseGun1 != null) activeGunIndex = 1;
                }

                UtillLogRemove.Log("총알이 다 떨어져서 무기를 버렸습니다!");

                // 1번 과 2번 총기슬롯이 비었으면
                if (UseGun1 == null && UseGun2 == null)
                {
                    // 데이터 매니저에서 기본권총 정보를 받아와 저장
                    WeaponData ptGunData = GameDataManager.Instance.GetWeaponData("Weapon_PT_1");

                    if (ptGunData != null) // 기본 권총이 있으면
                    {
                        UseGun1 = ptGunData; // 1번 스롯 총기에 저장
                        gun1Magazine = ptGunData.Capacity; // 1번 슬롯 탄창 채우기
                        gun1Reserve = ptGunData.Capacity2; // 1번 슬롯 예비 총알 채우기
                        activeGunIndex = 1; // 1번 슬롯 번호 지정

                        UtillLogRemove.Log("모든 무기를 소모하여 품속에서 기본 무기(PT)를 꺼냅니다.");
                    }
                }
                UpdateItemUI(); // 무기가 바뀌었으니 UI 갱신
            }
            return true; // 반환, 진실
        }
        return false; // 반환, 거짓
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

        UpdateItemUI();

        UtillLogRemove.Log($"{activeGunIndex}번 총기로 교체 완료!");
    }

    // 폭탄 
    public void UseBoomItem(bool isFaceRight)
    {
        // 폭탄 없으면 반환
        if (UseBoom == null) return;

        // 수류탄 프립팹이 있고, 생성 위치가 있으면
        if (grenedPrefab != null && thowPoint != null)
        {
            // 수륙탄 오브젝트를 수류탄 프리팹으로 생성위치에 회전 없이 생성
            GameObject boomObj = Instantiate(grenedPrefab, thowPoint.position, Quaternion.identity);
            // 수륙탄 매니저에서 컴포넌트 가져오기
            GrenadeManager grenadeLogic = boomObj.GetComponent<GrenadeManager>();

            if (grenadeLogic != null) // 컴포넌트 있으면
            {
                // 방향 확인해서 저장
                float dirX = isFaceRight ? 1f : -1f;
                grenadeLogic.Toss(dirX);
            }
        }

        UtillLogRemove.Log("폭탄 투척!");
        // 폭탄 삭제
        UseBoom = null;
        // UI아이템 업데이트 함수 호출
        UpdateItemUI();
    }
}
