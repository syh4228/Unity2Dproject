using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("프로필 : 체력바")]
    [SerializeField] private Text hpText; // 텍스트로 표기할 HP
    [SerializeField] private Image hpFillImage; // 체력바 표기할 HP
    [SerializeField] private Image hpTempFillImage; // 임시 체력바 표기할 HP
    [SerializeField] private Color tempHpColor; // 임시 체력바 고정 색깔

    [Header("총기 UI")]
    [SerializeField] private Text gun01_MagazineText; // 현재 탄창 총알 표기
    [SerializeField] private Text gun01_ReserveText; // 총 총알 표기
    [SerializeField] private Text gun02_MagazineText;
    [SerializeField] private Text gun02_ReserveText;

    [Header("아이템 슬롯")]
    [SerializeField] private Image grenadeImage; // 투척류 아이템 표기
    [SerializeField] private Image medkitImage; // 힐팩 아이템 표기
    [SerializeField] private Image pillsImage; // 임시 체력 회복 템 표기

    // 체력바에서 사용할 색깔 
    private Color healthyColor = Color.green;  // 안전 그린
    private Color warningColor = Color.yellow; // 주의 노랑
    private Color dangerColor = Color.red; // 경고 빨강

    // 체력 UI함수(현재체력, 임시체력, 최대체력)
    public void UpdateHealthUI(int currentHp, int tempHp, int maxHp)
    {
        int totalHp = currentHp + tempHp; // 총 체력은 현재체력 + 임시 체력

        float totalHpRatio = (float)totalHp / maxHp; // 토탈 체력 비율
        float realHpRatio = (float)currentHp / maxHp; // 실제 체력 비율

        if (hpText != null) // 널 아니면
        {
            hpText.text = totalHp.ToString(); // 토탈체력 Hp 표기 텍스트에 저장

            if (totalHpRatio > 0.5f) // 만약 체력 비율이 0.5f 보다 높으면
            {
                hpText.color = healthyColor; // hp 표기 텍스트 색 그린
            }
            else if (totalHpRatio < 0.2f ) // 만약 0.2f 보다 낮으면
            {
                hpText.color = warningColor; // 노랑
            }
            else // 둘다 아니면
            {
                hpText.color = dangerColor; // 빨강
            }
        }

        if (hpTempFillImage != null) // 널 아니면
        {
            hpTempFillImage.fillAmount = totalHpRatio; // 임시 체력바 그리기
            hpTempFillImage.color = tempHpColor; // 체력바 색깔 지정
        }

        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = realHpRatio; // 체력바 그리기

            if (realHpRatio > 0.5f) // 만약 체력 비율이 0.5f 보다 높으면
            {
                hpText.color = healthyColor; // hp 표기 텍스트 색 그린
            }
            else if (realHpRatio < 0.2f) // 만약 0.2f 보다 낮으면
            {
                hpText.color = warningColor; // 노랑
            }
            else // 둘다 아니면
            {
                hpText.color = dangerColor; // 빨강
            }
        }
    }

    public void UpdateAmmoUI(int gunSlot, int magazine, int reserve)
    {
        if (gunSlot == 1) // 총 슬롯 1일때
        {
            if (gun01_MagazineText != null) // 널 체크
            {
                gun01_MagazineText.text = magazine.ToString(); // 텍스트 추력
            }
        }
        if (gunSlot == 1)
        {
            if (gun01_ReserveText != null)
            {
                gun01_ReserveText.text = reserve.ToString();
            }
        }

        if (gunSlot == 2)
        {
            if (gun02_MagazineText != null)
            {
                gun02_MagazineText.text = magazine.ToString();
            }
        }

        if (gunSlot == 2)
        {
            if (gun02_ReserveText != null)
            {
                gun02_ReserveText.text = reserve.ToString();
            }
        }
    }

    // 아이템 슬롯 UI함수 ( 투척류, 회복 킷, 임시 회복탬)
    public void UpdateItemUI(bool hasgrenade, bool hasmedkit, bool haspills)
    {
        if (grenadeImage != null) // 널 체크
        {
            if (hasgrenade == true) // 투척물 가지고 있으면
            {
                grenadeImage.gameObject.SetActive(true); // 이미지 활성화
            }
        }

        if (medkitImage != null)
        {
            if (hasmedkit == true) // 매디킷 가지고 있으면
            {
                medkitImage.gameObject.SetActive(true); // 이미지 활성화
            }
        }

        if (pillsImage != null)
        {
            if (haspills == true) // 임시 회복탬 가지고 있으면
            {
                pillsImage.gameObject.SetActive(true); // 이미지 활성화
            }
        }
    }
}
