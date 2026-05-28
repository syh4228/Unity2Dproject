using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;

public class ItemBookSlotUI : MonoBehaviour
{
    [Header("기본 슬롯 정보")]
    [SerializeField] private Image Image_SlotIcon; // 이미지
    [SerializeField] private GameObject GameObject_check; // 체크 이미지(활성 비활성화 용)
    [SerializeField] private UIButton Button_SlotClick; // 버튼 연결

    private event Action<string, EGameBookCategory> _onClickSlot; // 클릭이벤트 변수

    private string _slotDataId; // 슬롯 데이터 저장 변수

    private EGameBookCategory _curSlotCategory; // 데이터 보관
    
    public string GetSlotDataId() // 슬롯 데이터 아이디 주는 함수
    {
        return _slotDataId;
    }

    private void OnEnable()
    {
        // UI버튼 컴포넌트 이벤트 클릭 함수 호출 => 구독
        Button_SlotClick.BindOnClickButtonEvent(OnClick_GameBookSlot);
    }

    // 버튼 눌렸을때 이벤트 함수
    public void OnClick_GameBookSlot()
    {
        if (_onClickSlot != null)
        {
            // 자식이 눌러졌지만, 부모한테 알림
            _onClickSlot.Invoke(_slotDataId, _curSlotCategory);
        }

    }

    private void OnDisable()
    {
        _onClickSlot = null;
    }

    // 부모인 북UI에 전달해주는 함수
    public void initSlot(string dataId, EGameBookCategory curCategory ,Action<string, EGameBookCategory> onClickCallback)
    {
        if (curCategory == EGameBookCategory.ItemCategory)
        {
            var itemData = GameDataManager.Instance.GetWeaponData(dataId);

            if (itemData == null)
            {
                return;
            }

            // 만약 슬롯 기본 정보에서 텍스트를 연결해줘야 할게 있었다면 추가
            // Text_MainName.text = itemData.Name; // 이름 반영

            string iconPath = itemData.IconPath;

            if (string.IsNullOrEmpty(iconPath) == false)
            {
                GameUtill.LoadAndSetSpriteImage(Image_SlotIcon, iconPath).Forget();
            }
        }
        else if ( curCategory == EGameBookCategory.MonsterCategory)
        {
            var MonsterData = GameDataManager.Instance.GetDNMonsterData(dataId);

            if (MonsterData == null)
            {
                return;
            }

            string iconPath = MonsterData.IconPath;

            if (string.IsNullOrEmpty(iconPath) == false)
            {
                GameUtill.LoadAndSetSpriteImage(Image_SlotIcon, iconPath).Forget();
            }
        }

        // 데이터 저장
        _slotDataId = dataId;

        _curSlotCategory = curCategory;

        _onClickSlot += onClickCallback; // 구독
    }

    public void SetSelectedUI(bool isSelect)
    {
        GameObject_check.SetActive(isSelect);
    }
}
