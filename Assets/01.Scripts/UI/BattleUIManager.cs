using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    [Header("프로필 : 체력바")]
    [SerializeField] private TextMeshProUGUI hpText; // 텍스트로 표기할 HP
    [SerializeField] private Image hpFillImage; // 체력바 표기할 HP
    [SerializeField] private Image hpTempFillImage; // 임시 체력바 표기할 HP
    [SerializeField] private Color tempHpColor; // 임시 체력바 고정 색깔
    
    [Header("총기 UI")]
    [SerializeField] private TextMeshProUGUI gun01_MagazineText; // 현재 탄창 총알 표기
    [SerializeField] private TextMeshProUGUI gun01_ReserveText; // 총 총알 표기
    [SerializeField] private TextMeshProUGUI gun02_MagazineText;
    [SerializeField] private TextMeshProUGUI gun02_ReserveText;

    [Header("아이템 슬롯")]
    [SerializeField] private RawImage grenadeImage; // 투척류 아이템 표기
    [SerializeField] private RawImage medkitImage; // 힐팩 아이템 표기
    [SerializeField] private RawImage pillsImage; // 진통제 표기
    [SerializeField] private RawImage adrenalineImage; // 아드레날린 표기

    [Header("나가기 팝업")]
    [SerializeField] private GameObject exitpopup; // 나가기창 연결

    [Header("타겟 UI")]
    [SerializeField] private GameObject targetNoramlUI; // 노멀 타켓 연결
    [SerializeField] private GameObject targetSpecialUI; // 스폐셜 타겟 연결

    [Header("총기 이미지")]
    [SerializeField] private Image gun01_Image; // 1번 총기 이미지
    [SerializeField] private Image gun02_Image; // 2번 총기 이미지

    [Header("마운트 알림")]
    [SerializeField] private GameObject mountAlertImage;

    [Header("튜토리얼 UI")]
    [SerializeField] private GameObject tutorialBK; // 튜토리얼 배경 (BK)
    [SerializeField] private GameObject tt_01;      // 첫 번째 튜토리얼 오브젝트
    [SerializeField] private GameObject tt_02;      // 두 번째 튜토리얼 오브젝트

    private int tutorialStep = 0;

    // 체력바에서 사용할 색깔 
    private Color healthyColor = Color.green;  // 안전 그린
    private Color warningColor = Color.yellow; // 주의 노랑
    private Color dangerColor = Color.red; // 경고 빨강

    private void Update()
    {
        // 튜토리얼이 진행 중일 때 엔터키(Return) 혹은 숫자패드 엔터(KeypadEnter) 입력 감지
        if (tutorialStep > 0 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            NextTutorialStep();
        }
    }

    private void Start()
    {
        // 게임 시작 시 튜토리얼을 강제로 1단계부터 시작하게 만듭니다.
        StartTutorial();
    }

    public void StartTutorial()
    {
        tutorialStep = 1; // 1단계로 설정
        if (tutorialBK != null) tutorialBK.SetActive(true);
        if (tt_01 != null) tt_01.SetActive(true);
        if (tt_02 != null) tt_02.SetActive(false);
    }

    private void NextTutorialStep()
    {
        if (tutorialStep == 1)
        {
            // 1단계 완료 -> 2단계 시작
            if (tt_01 != null) tt_01.SetActive(false);
            if (tt_02 != null) tt_02.SetActive(true);
            tutorialStep = 2;
        }
        else if (tutorialStep == 2)
        {
            // 2단계 완료 -> 튜토리얼 최종 종료
            if (tt_02 != null) tt_02.SetActive(false);
            if (tutorialBK != null) tutorialBK.SetActive(false); // 배경도 함께 끄기
            tutorialStep = 0;
        }
    }

    // 체력 UI함수(현재체력, 임시체력, 최대체력)
    public void UpdateHealthUI(int currentHp, int tempHp, int maxHp)
    {
        int totalHp = currentHp + tempHp; // 총 체력은 현재체력 + 임시 체력

        float totalHpRatio = (float)totalHp / maxHp; // 토탈 체력 비율
        float realHpRatio = (float)currentHp / maxHp; // 실제 체력 비율

        if (hpText != null)
        {
            hpText.text = totalHp.ToString();
            // 텍스트 색상 결정
            hpText.color = GetHealthColor(totalHpRatio);
        }

        // 임시 체력바 적용
        if (hpTempFillImage != null)
        {
            bool hasTempHp = (tempHp > 0);

            if (hpTempFillImage.gameObject.activeSelf != hasTempHp)
            {
                hpTempFillImage.gameObject.SetActive(hasTempHp);
            }

            if (hasTempHp)
            {
                hpTempFillImage.fillAmount = totalHpRatio;
                hpTempFillImage.color = tempHpColor;
            }
        }

        // 4. 실제 체력바 적용
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = realHpRatio;
            // 실제 체력 색상 결정
            hpFillImage.color = GetHealthColor(realHpRatio);
        }
    }

    // 체력표기 색깔 정하는 함수
    private Color GetHealthColor(float ratio)
    {
        if (ratio > 0.5f) return healthyColor;
        if (ratio < 0.2f) return warningColor;
        return dangerColor;
    }

    public void UpdateAmmoUI(int gunSlot, int magazine, int reserve)
    {
        // 예비탄창이 -1이면 무한으로 표기
        string resText = (reserve == -1) ? "∞" : reserve.ToString(); 

        if (gunSlot == 1) // 총 슬롯 1일때
        {
            if (gun01_MagazineText != null) // 널 체크
            {
                gun01_MagazineText.text = magazine.ToString(); // 텍스트 출력
            }

            if (gun01_ReserveText != null)
            {
                gun01_ReserveText.text = resText;
            }
        }

        if (gunSlot == 2)
        {
            if (gun02_MagazineText != null)
            {
                gun02_MagazineText.text = magazine.ToString();
            }

            if (gun02_ReserveText != null)
            {
                gun02_ReserveText.text = resText;
            }
        }
    }

    // 아이템 슬롯 UI함수 ( 투척류, 회복 킷, 임시 회복탬)
    public void UpdateItemUI(bool hasGrenade, bool hasMedkit, bool hasPills, bool hasAdrenaline)
    {
        if (grenadeImage != null) // 널 체크
        {
            // 슈륙탄 가지고 있으면, true, 아니면 false
            grenadeImage.gameObject.SetActive(hasGrenade);
        }

        if (medkitImage != null) // 매디 킷
        {
            medkitImage.gameObject.SetActive(hasMedkit);
        }

        if (pillsImage != null) // 진통제
        {
            pillsImage.gameObject.SetActive(hasPills);
        }

        if (adrenalineImage != null) // 아드레날린
        {
            adrenalineImage.gameObject.SetActive(hasAdrenaline);
        }
    }

    // 총기 슬롯 업데이트 함수
    public async void UpdateWeaponSlotUI(int slotIndex, string iconPath, bool isActive)
    {
        // 쓰고 있는 슬롯이 1번이면 1번 이미지를 ,2번이면 2번이미지를 저장
        Image targetImage = (slotIndex == 1) ? gun01_Image : gun02_Image;

        // 없으면 반환
        if (targetImage == null) return;


        float alpha = isActive ? 1f : 0.4f;

        // 경로가 비어있거나, 안적혀있으면
        if (string.IsNullOrEmpty(iconPath))
        {
            targetImage.sprite = null; // 스프라이트 없으면
            targetImage.color = new Color(1, 1, 1, 0); // 투명하게
            return;
        }

        targetImage.color = new Color(1, 1, 1, alpha);

        Sprite weaponSprite = null;

        try
        {
            // 어드레서블에서 아이콘 찾기
            weaponSprite = await ResourceManager.Inst.LoadSprite(iconPath);
        }
        catch { }

        if (weaponSprite == null)
        {
            weaponSprite = Resources.Load<Sprite>(iconPath);
        }

        if (weaponSprite != null)
        {
            targetImage.sprite = weaponSprite;
        }
        else
        {
            Debug.LogError($"[UI] 아이콘 로드 실패: {iconPath}");
        }
    }

    // 나가기 팝업 관리 함수
    public void ShowExitPopup(bool isActive)
    {
        if (exitpopup  != null) // 팝업이 있으면
        {
            // 팝업 켜고,크기
            exitpopup.SetActive(isActive);
        }
    }

    // 팝업의 상태 확인 함수
    public bool IsExitPopupActive()
    {
        if (exitpopup != null) // 팝업이 있으면
        {
            // 현재 팝업이 켜져있는지 꺼져있느지 확인
            return exitpopup.activeSelf;
        }

        // 끄기
        return false;
    }

    // 나가기 버튼 이벤트 함수
    public void OnClick_ConfirmExit()
    {
        ShowExitPopup(false); // 팝업 끄기

        if (UIManager.Instance != null) // UI매니저가 있으면
        {
            // UI매니저에서 나가기 함수 호출
            UIManager.Instance.RequestExitBattle();
        }
    }

    // 타겟 UI 업데이트 함수
    public void UpdateTargetUI(ZombieType targetType)
    {
       // 타겟 타입이 좀비 노멀이면 isNormal이 true
       bool isNormal = (targetType == ZombieType.Normal);

        if (targetNoramlUI != null)
        {
            targetNoramlUI.SetActive(isNormal); // true면 노멀 타겟 활성화
        }

        if (targetSpecialUI != null)
        {
            targetSpecialUI.SetActive(!isNormal); //false면 스폐셜 타겟 활성화
        }

        UtillLogRemove.Log($"UI 갱신 {(isNormal ? "Normal" : "Special")}");
    }

    public void SetMountAlert(bool isActive)
    {
        if (mountAlertImage != null)
        {
            mountAlertImage.SetActive(isActive);
        }
    }
}
