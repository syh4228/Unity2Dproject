using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

public enum EGameBookCategory
{
    None = 0,
    ItemCategory,
    MonsterCategory
}
public class ItemBookUI : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private GameObject Prefab_Slot; // 동적 생성 프리팹

    [Header("페이지(패널) 영역")]
    [SerializeField] private GameObject Panel_ItemDetail;    // 아이템 전용 상세 페이지
    [SerializeField] private GameObject Panel_MonsterDetail; // 몬스터 전용 상세 페이지

    [Header("아이템 기본 정보 영역")]
    [SerializeField] private Image Image_ItemIcon; // 메인 아이콘
    [SerializeField] private TextMeshProUGUI Text_ItemName; // 메인 이름
    [SerializeField] private TextMeshProUGUI Text_ItemDescription; // 설명

    [Header("몬스터 상세 정보 영역")]
    [SerializeField] private Image Image_MonsterIcon; // 몬스터 전용 아이콘
    [SerializeField] private TextMeshProUGUI Text_MonsterName; // 몬스터 전용 이름
    [SerializeField] private TextMeshProUGUI Text_MonsterDescription; // 몬스터 전용 설명

    [Header("카테고리 영역")]
    [SerializeField] private UIButton Button_ItemCategory; // 아이템 카테고라
    [SerializeField] private UIButton Button_MonsterCategory; // 몬스터 카테고리

    [Header("상세 정보 영역")]
    [SerializeField] private TextMeshProUGUI Text_Damage; // 데미지
    [SerializeField] private Image Image_DamageBar; // 데미지 이미지 바
    [SerializeField] private TextMeshProUGUI Text_RPM; // 사속
    [SerializeField] private Image Image_RPMBar;
    [SerializeField] private TextMeshProUGUI Text_ER; // 사거리
    [SerializeField] private Image Image_ERBar;
    [SerializeField] private TextMeshProUGUI Text_Capacity; // 총알

    [Header("슬롯 리스트 영역")]
    [SerializeField] private Transform Transform_SlotRoot; // 스롯이 생성될 곳

    [Header("닫기 버튼")]
    [SerializeField] private UIButton Button_CloseUI; // 닫기 번튼 연결

    // 딕셔너리로 저장관리
    private Dictionary<string, ItemBookSlotUI> _slotList = new Dictionary<string, ItemBookSlotUI>();

    private void OnEnable()
    {
        // UI가 열릴때 스스로, 기본적인 아이템 도감안에 있는 모든 데이터 불러오기
        OnClick_ItemCategory();

        if (Button_CloseUI != null)
        {
            Button_CloseUI.BindOnClickButtonEvent(OnClick_CloseGameBookUI);
        }

        if (Button_ItemCategory != null)
        {
            Button_ItemCategory.BindOnClickButtonEvent(OnClick_ItemCategory);
        }

        if (Button_MonsterCategory != null)
        {
            Button_MonsterCategory.BindOnClickButtonEvent(OnClick_MonsterCategory);
        }
    }

    private void OnDisable()
    {
        if (Button_CloseUI != null)
        {
            Button_CloseUI.UnBindOnClickButtonEvent(OnClick_CloseGameBookUI);
        }

        OnDestroyAndClearSlotList();
    }

    private void OnDestroyAndClearSlotList()
    {
        if (_slotList.Count > 0)
        {
            foreach (var slotKv in _slotList) // 하나씩 꺼내서 
            {
                var slot = slotKv.Value; // 컴포넌트지만, 게임오브젝트로 받을 수 있다.
                DestroyImmediate(slot.gameObject); // 오브젝트 슬롯 파괴
            }

            _slotList.Clear();
        }
    }

    public void OnClick_CloseGameBookUI()
    {
        this.gameObject.SetActive(false);
    }

    public void OnClick_ItemCategory()
    {
        SetGameBookCategory(EGameBookCategory.ItemCategory);
    }

    public void OnClick_MonsterCategory()
    {
        SetGameBookCategory(EGameBookCategory.MonsterCategory);
    }

    private void SetGameBookCategory(EGameBookCategory catgory)
    {
        OnDestroyAndClearSlotList();

        switch (catgory)
        {
            case EGameBookCategory.ItemCategory:

                // 아이템 패널은 켜고, 몬스터 패널은 끄기
                if (Panel_ItemDetail != null) Panel_ItemDetail.SetActive(true);
                if (Panel_MonsterDetail != null) Panel_MonsterDetail.SetActive(false);

                ReadItemListAndCreateSlot(); 
                break;
            case EGameBookCategory.MonsterCategory:

                // 몬스터 패널은 켜고, 아이템 패널은 끄기
                if (Panel_ItemDetail != null) Panel_ItemDetail.SetActive(false);
                if (Panel_MonsterDetail != null) Panel_MonsterDetail.SetActive(true);

                ReadMonterListAndCreateSlot();
                break;
            default:
                break;
        }
    }

    // 아이템 리스트를 읽고 슬롯 생성 함수
    private void ReadItemListAndCreateSlot() 
    {
        // 게임데이터 매니저에서 웨폰 리스트 가져오기
        var dataList = GameDataManager.Instance.WeaponDataList;

        foreach (var datakv in dataList) // Kv => K = Key, v = Value
        {
            var data = datakv.Value;

            if (data == null)
            {
                continue;
            }

            CreateGameBookSlot(data.Id, EGameBookCategory.ItemCategory); // 슬롯 생성 함수에 아이 전달
        }

        if (_slotList.Count > 0) // 도감 열떄 한나 도감 내용 활성화
        {
            foreach (var slotKv in _slotList)
            {
                var slot = slotKv.Value;
                slot.OnClick_GameBookSlot(); 
            }
        }
    }

    private void ReadMonterListAndCreateSlot()
    {
        var dataList = GameDataManager.Instance.DNMonsterDataList;

        foreach (var datakv in dataList) 
        {
            var data = datakv.Value;

            if (data == null)
            {
                continue;
            }

            CreateGameBookSlot(data.Id, EGameBookCategory.MonsterCategory);
        }

        if (_slotList.Count > 0) 
        {
            foreach (var slotKv in _slotList)
            {
                var slot = slotKv.Value;
                slot.OnClick_GameBookSlot();
            }
        }
    }

    // 슬롯 생성 함수(id) -> 슬롯 1개만 생성해주는 로직
    private void CreateGameBookSlot(string dataId, EGameBookCategory curCategory)
    {
        // 인스탄티드
        var gObj = Instantiate(Prefab_Slot, Transform_SlotRoot);

        if (gObj == null)
        {
            return;
        }

        // 게임오브젝트 동적생성 (아이템북 슬롯 컴포넌트 가져오기)
        var slotComponent = gObj.GetComponent<ItemBookSlotUI>();

        if (slotComponent == null)
        {
            return;
        }

        // 자식에서 슬롯 정보 가져오기
        slotComponent.initSlot(dataId, curCategory, OnClickchuldSlotSelected);
        _slotList.Add(dataId, slotComponent);
    }

    // 슬롯 버튼이 눌러졌을때 자식한테서 데이터 받아오기
    private void OnClickchuldSlotSelected(string slotDataId, EGameBookCategory  selectedSlotCategory)
    {
        // 아이템 카테고리
        if (selectedSlotCategory == EGameBookCategory.ItemCategory)
        {
            var itemData = GameDataManager.Instance.GetWeaponData(slotDataId);

            if (itemData == null)
            {
                return;
            }

            Text_ItemName.text = itemData.Name;
            Text_ItemDescription.text = itemData.Description;

            if (string.IsNullOrEmpty(itemData.IconPath) == false)
            {
                GameUtill.LoadAndSetSpriteImage(Image_ItemIcon, itemData.IconPath).Forget();
            }

            Text_Damage.text = itemData.Damage.ToString();
            Text_RPM.text = itemData.RPM.ToString();
            Text_ER.text = itemData.EffectiveRange.ToString();

            // 장탄수 표기
            if (itemData.Capacity2 == -1)
            {
                Text_Capacity.text = $"{itemData.Capacity} / ∞";
            }
            else if (itemData.Capacity == 0 && itemData.Capacity2 == 0)
            {
                Text_Capacity.text = "-";
            }
            else
            {
                Text_Capacity.text = $"{itemData.Capacity} / {itemData.Capacity2}";
            }

            // 이미지 바 표기
            float maxDamage = 100f;
            Image_DamageBar.fillAmount = itemData.Damage / maxDamage;

            float maxRPM = 100f;
            Image_RPMBar.fillAmount = itemData.RPM / maxRPM;

            float maxER = 100f;
            Image_ERBar.fillAmount = itemData.EffectiveRange / maxER;
        }
        // 몬스터 카테고리
        else if (selectedSlotCategory == EGameBookCategory.MonsterCategory)
        {
            var MonsterData = GameDataManager.Instance.GetDNMonsterData(slotDataId);

            if (MonsterData == null)
            {
                return;
            }

            Text_MonsterName.text = MonsterData.Name;
            Text_MonsterDescription.text = MonsterData.Description;

            if (string.IsNullOrEmpty(MonsterData.IconPath) == false)
            {
                GameUtill.LoadAndSetSpriteImage(Image_MonsterIcon, MonsterData.IconPath).Forget();
            }
            // 가져올 아이콘이미지 없으면 이미지 비활성화
            Image_MonsterIcon.gameObject.SetActive(string.IsNullOrEmpty(MonsterData.IconPath) == false);
        }

        foreach (var slotKv in _slotList) // 하나씩 확인하면서 활성화, 비활성화
        {
            var slot = slotKv.Value;
            var dataId = slot.GetSlotDataId();
            slot.SetSelectedUI(slotDataId == dataId);
        }
    }
}
