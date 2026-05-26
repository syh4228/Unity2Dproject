using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class ItemBookSlotUI : MonoBehaviour
{
    [Header("기본 슬롯 정보")]
    [SerializeField] private Image Image_SlotIcon; // 이미지
    [SerializeField] private GameObject GameObject_check; // 체크 이미지(활성 비활성화 용)

    private string _slotDataId; // 슬롯 데이터 저장 변수

    // 부모인 북UI에 전달해주는 함수
    public void initSlot(string dataId)
    {
        var itemData = GameDataManager.Instance.GetWeaponData(dataId);

        if (itemData == null)
        {
            return;
        }

        // 만약 슬롯 기본 정보에서 텍스트를 연결해줘야 할게 있었다면 추가
        // Text_MainName.text = itemData.Name; // 이름 반영

        string iconPath = itemData.IconPath;

        if (string.IsNullOrEmpty(iconPath) == true)
        {
            return;
        }

        GameUtill.LoadAndSetSpriteImage(Image_SlotIcon, iconPath).Forget();

        // 데이터 저장
        _slotDataId = dataId;
    }
}
