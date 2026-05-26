using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBookUI : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private GameObject Prefab_Slot; // 동적 생성 프리팹

    [Header("기본 정보 영역")]
    [SerializeField] private Image Image_MainIcon; // 메인 아이콘
    [SerializeField] private Text Text_MainName; // 메인 이름
    [SerializeField] private Text Text_Description; // 설명

    [Header("상세 정보 영역")]
    [SerializeField] private Text Text_Damage; // 데미지
    [SerializeField] private Image Image_DamageBar; // 데미지 이미지 바
    [SerializeField] private Text Text_RPM; // 사속
    [SerializeField] private Image Image_RPMBar;
    [SerializeField] private Text Text_ER; // 사거리
    [SerializeField] private Image Image_ERBar;
    [SerializeField] private Text Text_Capacity; // 총알

    [Header("슬롯 리스트 영역")]
    [SerializeField] private Transform Transform_SlotRoot; // 스롯이 생성될 곳

    // 딕셔너리로 저장관리
    private Dictionary<string, ItemBookSlotUI> _slotList = new Dictionary<string, ItemBookSlotUI>();

    private void OnEnable()
    {
        // UI가 열릴때 스스로, 기본적인 아이템 도감안에 있는 모든 데이터 불러오기
        ReadItemListAndCreateSlot();

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

            CreateGameBookSlot(data.Id); // 슬롯 생성 함수에 아이 전달
        }
    }

    // 슬롯 생성 함수(id) -> 슬롯 1개만 생성해주는 로직
    private void CreateGameBookSlot(string dataId)
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
        slotComponent.initSlot(dataId);
        _slotList.Add(dataId, slotComponent);
    }
}
