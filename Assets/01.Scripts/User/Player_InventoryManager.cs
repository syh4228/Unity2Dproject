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

    private async void Start()
    {
        if (UiManager == null)
        {
            UiManager = FindAnyObjectByType<BattleUIManager>(FindObjectsInactive.Include);
        }

        if (UiManager == null)
        {
            Debug.LogError("현재 씬(맵) 안에 'BattleUIManager' 스크립트가 붙어있는 오브젝트가 아예 없습니다!");
        }

        await Cysharp.Threading.Tasks.UniTask.Delay(100);

        // 게임데이터 매니저에서 기본 권총 1의 정보를 가져와 저장
        WeaponData ptGunData = GameDataManager.Instance.GetWeaponData("Weapon_PT_1");

        if (ptGunData != null ) // 권총이 있으면
        {
            UseGun1 = ptGunData; // 1번 무기 슬롯에 권총 장착
            gun1Magazine = ptGunData.Capacity; // 탄창 채우기
            gun1Reserve = ptGunData.Capacity2; // 예비 탄창 채우기
            activeGunIndex = 1; // 무기 1번으로 설정

            UpdateItemUI(); // UI 갱신
        }
    }

    // 획득 아이템 관리 함수
    public void PickUpItem(string itemId)
    {
        // 데이터 매니저에서 주운 아이템 정보 가져오기
        var itemData = GameDataManager.Instance.GetWeaponData(itemId);

        if (itemData == null) return;

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
        else if (useType == "RepleAC")
        {
            RefillAmmo(); // 탄약 보충
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
                activeGunIndex = 2;
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
                activeGunIndex = 1;
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
        if (id.Contains("HK") == true) // 메디킷
        {
            if (UseHeel1 == null)
            {
                UseHeel1 = itemData;
                UpdateItemUI();
            }
            else Debug.Log("이미 구급상자가 있습니다!");
        }
        else if (id.Contains("MD") == true || id.Contains("AD") == true) // 진통제, 아드레날린
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

    // 힐 킷 보유 확인 함수
    public bool HasHeelItem1()
    {
        if (UseHeel1 != null)
        {
            return true;
        }
        return false;
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

    // 진통제, 아드레날린 사용 함수
    public int UseHeelItem2()
    {
        if (UseHeel2 == null) return 0;

        if (UseHeel2.Id.Contains("MD") == true)
        {
            PlayerCharacter.ApplyHeal_MD();
            UseHeel2 = null;
            UpdateItemUI();
            return 1;
        }
        else if (UseHeel2.Id.Contains("AD") == true)
        {
            PlayerCharacter.ApplyHeal_AD();
            UseHeel2 = null;
            UpdateItemUI();
            return 2;
        }
        return 0;
    }

    // 아이템 UI 업데이트
    private void UpdateItemUI()
    {
        // 아이템이 전부 있을때, UI 업데이트
        bool hasGrenade = (UseBoom != null);
        bool hasMedkit = (UseHeel1 != null);
        bool hasPills = false;
        bool hasAdrenaline = false;

        if (UseHeel2 != null)
        {
            if (UseHeel2.Id.Contains("MD") == true)
            {
                hasPills = true;
            }
            else if (UseHeel2.Id.Contains("AD") == true)
            {
                hasAdrenaline = true;
            }
        }

        UiManager.UpdateItemUI(hasGrenade, hasMedkit, hasPills, hasAdrenaline);

        UpdateGunSlotUI(1, UseGun1, gun1Magazine, gun1Reserve, activeGunIndex == 1);
        UpdateGunSlotUI(2, UseGun2, gun2Magazine, gun2Reserve, activeGunIndex == 2);

        WeaponData currentGun;
        if (activeGunIndex == 1)
        {
            currentGun = UseGun1;
        }
        else
        {
            currentGun = UseGun2;
        }

        if (currentGun != null && AnimController != null)
        {
            AnimController.ChangeWeaponAnimation(currentGun.Anim_AttackPath, currentGun.Anim_ReloadPath).Forget();
            AnimController.SetAttackSpeed(currentGun.RPM);
        }
    }

    // 건슬롯UI업데이트
    private void UpdateGunSlotUI(int slot, WeaponData gun, int mag, int res, bool isActive)
    {
        if (gun != null)
        {
            UiManager.UpdateWeaponSlotUI(slot, gun.IconPath, isActive);

            int displayReserve;
            if (gun.Capacity2 == -1)
            {
                displayReserve = -1;
            }
            else
            {
                displayReserve = res;
            }

            UiManager.UpdateAmmoUI(slot, mag, displayReserve);
        }
        else
        {
            UiManager.UpdateWeaponSlotUI(slot, null, false);
            UiManager.UpdateAmmoUI(slot, 0, 0);
        }
    }

    // 총 발사 함수
    public bool TryFireCurrentGun(bool isLookLeft)
    {
        // 내가 사용중에 있는 총기가 있고, 그게 1번 총기 인지 2번총기 인지 확인해서 저장
        WeaponData currentGun = (activeGunIndex == 1) ? UseGun1 : UseGun2;

        if (currentGun == null) return false; // 현재 총이 없으면 반환

        // 내가 사용중인 총기가 1번 총기의 탄창인지, 2번 총기의 탄창인지 확인해서 저장
        ref int currentMag = ref (activeGunIndex == 1) ? ref gun1Magazine : ref gun2Magazine;
        // 내가 사용중인 총기가 1번 총기의 예비 탄창인지, 2번 총기의 예비 탄창인지 확인해서 저장
        ref int currentRes = ref (activeGunIndex == 1) ? ref gun1Reserve : ref gun2Reserve;

        if (currentMag <= 0) // 총알이 0보다 적으면
        {
            ReloadCurrentGun(); // 재장전 함수 호출
            AnimController.SetState(AllState.Reload);
            return false; // 반환  거짓
        }

        // 발사 방향, 총 대미지, 대미지 타입, 사거리, 사속 웨폰메니저에서 받아서 저장
        bool isFired = WeaponManager.FireBullet(isLookLeft, currentGun);

        if (isFired == true) // 발사가 트루면
        {
            currentMag--; // 총알 감소

            // UI 매니저에 총알 소모 알림
            UiManager.UpdateAmmoUI(activeGunIndex, currentMag, currentRes);

            // 예비탄창이 -1이 아니고, 탄창이 0 이고, 예비 총알이 0이면
            if (currentGun.Capacity2 != -1 && currentMag == 0 && currentRes == 0)
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
                    WeaponData pt = GameDataManager.Instance.GetWeaponData("Weapon_PT_1");

                    UseGun1 = pt;
                    gun1Magazine = pt.Capacity;
                    gun1Reserve = pt.Capacity2;
                    activeGunIndex = 1;
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

        if (need <= 0) return; // 탄창 차있으면 반환

        // 예비 탄창이 -1 이면
        if (currentGun.Capacity2 == -1) 
        {
            magazine += need; // 필요한 만큼 채우기
            // UI 업테이트
            UiManager.UpdateAmmoUI(activeGunIndex, magazine, -1);
            UtillLogRemove.Log("무한 탄창 장전 완료!");
        }
        // 예비 탄창이 0 보다 크면
        else if (reserve > 0)
        {
            // 필요한 양, 예비탄창 둘중에 적은 쪽 저장
            int reloadAmount = Mathf.Min(need, reserve);
            magazine += reloadAmount; // 탄창 채움
            reserve -= reloadAmount; // 예비 탄약 감소
            // UI 매니저 업데이트
            UiManager.UpdateAmmoUI(activeGunIndex, magazine, reserve);
            UtillLogRemove.Log($"일반 장전 완료! 남은 예비: {reserve}");
        }
        else
        {
            UtillLogRemove.Log("예비 탄창이 없어서 장전 불가!");
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
    public bool UseBoomItem(bool isFaceRight)
    {
        // 폭탄 없으면 반환
        if (UseBoom == null) return false;

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
        return true;
    }

    private void RefillAmmo()
    {
        // 총이 있고, 무한 탄창이 아니면
        if (UseGun1 != null && UseGun1.Capacity2 != -1)
        {
            // 예비탄창 채우기
            gun1Reserve = UseGun1.Capacity2;
        }

        if (UseGun2 != null && UseGun2.Capacity2 != -1)
        {
            gun2Reserve = UseGun2.Capacity2;
        }

        UtillLogRemove.Log("탄약 충전");
        UpdateItemUI();
    }
}
