using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class ItemBookUI : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private GameObject Prefab_Slot; // 동적 생성 프리팹

    [Header("기본 정보 영역")]
    [SerializeField] private Image Image_MainIcon; // 메인 아이콘
    [SerializeField] private TextMeshProUGUI Text_MainName; // 메인 이름
    [SerializeField] private TextMeshProUGUI Text_Description; // 설명

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
        ReadItemListAndCreateSlot();

        if (Button_CloseUI != null)
        {
            Button_CloseUI.BindOnClickButtonEvent(OnClick_CloseGameBookUI);
        }
    }

    public void OnClick_CloseGameBookUI()
    {
        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (Button_CloseUI != null)
        {
            Button_CloseUI.UnBindOnClickButtonEvent(OnClick_CloseGameBookUI);
        }

        if ( _slotList.Count > 0 )
        {
            foreach(var slotKv in _slotList) // 하나씩 꺼내서 
            {
                var slot = slotKv.Value; // 컴포넌트지만, 게임오브젝트로 받을 수 있다.
                DestroyImmediate(slot.gameObject); // 오브젝트 슬롯 파괴
            }

            _slotList.Clear();
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

            CreateGameBookSlot(data.Id); // 슬롯 생성 함수에 아이 전달
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
        slotComponent.initSlot(dataId, OnClickchuldSlotSelected);
        _slotList.Add(dataId, slotComponent);
    }

    // 슬롯 버튼이 눌러졌을때 자식한테서 데이터 받아오기
    private void OnClickchuldSlotSelected(string slotDataId)
    {
        var currentSelectedData = GameDataManager.Instance.GetWeaponData(slotDataId);
        
        if (currentSelectedData == null)
        {
            return;
        }


        GameUtill.LoadAndSetSpriteImage(Image_MainIcon, currentSelectedData.IconPath).Forget(); 
        Text_MainName.text = currentSelectedData.Name;
        Text_Description.text = currentSelectedData.Description;

        // int 형 답을 string으로 받기 위해서 To.String() 붙임
        Text_Damage.text = currentSelectedData.Damage.ToString();
        Text_RPM.text = currentSelectedData.RPM.ToString();
        Text_ER.text = currentSelectedData.EffectiveRange.ToString();
        // 장탄수 표기
        if (currentSelectedData.Capacity2 == -1)
        {
            // -1 이면 무한대 기호(∞)로 출력
            Text_Capacity.text = $"{currentSelectedData.Capacity} / ∞";
        }
        else if (currentSelectedData.Capacity == 0 && currentSelectedData.Capacity2 == 0)
        {
            // 둘 다 0이면 (근접무기, 회복약 등) 대시(-)로 출력
            Text_Capacity.text = "-";
        }
        else
        {
            // 일반 총기류는 정상적으로 "10 / 100" 형태로 출력
            Text_Capacity.text = $"{currentSelectedData.Capacity} / {currentSelectedData.Capacity2}";
        }

        // int형 답을 받아서 최대 값을 나눠서 이미지 바(fillAmount)에 방영
        float maxDamage = 100f; // 무기 최대 데미지 기준점
        Image_DamageBar.fillAmount = currentSelectedData.Damage / maxDamage;

        float maxRPM = 100f;    // 무기 최대 사속 기준점
        Image_RPMBar.fillAmount = currentSelectedData.RPM / maxRPM;

        float maxER = 100f;     // 무기 최대 사거리 기준점
        Image_ERBar.fillAmount = currentSelectedData.EffectiveRange / maxER;

        foreach (var slotKv in _slotList) // 하나씩 확인하면서 활성화, 비활성화
        {
            var slot = slotKv.Value;
            var dataId = slot.GetSlotDataId();
            slot.SetSelectedUI(slotDataId == dataId);
        }
    }
}
