using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomUIManager : MonoBehaviour
{
    [Header("리스트 연결")]
    [SerializeField] private GameObject campaignList; // 캠페인
    [SerializeField] private GameObject chapterList; // 챕터
    [SerializeField] private GameObject difficultyList; // 난이도
    [SerializeField] private GameObject characterList; // 캐릭터

    [Header("버튼 연결")]
    [SerializeField] private Button btnCampaign; // 캡페인 선택
    [SerializeField] private Button btnChapter; // 챕터 선택
    [SerializeField] private Button btnDifficulty; // 난이도 선택
    [SerializeField] private Button btnCharacter; // 캐릭터 선택
    [SerializeField] private Button btnStart; // 시작 버튼 선택
    [SerializeField] private Button btnBack; // 뒤로가기

    [Header("경고 팝업")]
    [SerializeField] private GameObject warningPopup; // 경고창 연결
    [SerializeField] private Button btnWarningClose;  // 경고창 버튼 연결

    [Header("선택 텍스트 연결")]
    [SerializeField] private TextMeshProUGUI txtCampaign; // 캠페인 글자
    [SerializeField] private TextMeshProUGUI txtChapter;  // 챕터 글자
    [SerializeField] private TextMeshProUGUI txtDifficulty; // 난이도 글자
    [SerializeField] private TextMeshProUGUI txtCharacter; // 캐릭터 글자

    [Header("색상 설정")]
    [SerializeField] private Color defaultColor = Color.white; // 기본 텍스트 색깔
    [SerializeField] private Color selectedColor = Color.green; // 선택 후 텍스트 색깔

    private GameObject currentActiveList; // 현재 활성화된 리스트 저장

    private void OnEnable() // 로비 ui가 다시 켜질때마다 색깔 초기화
    {
        if (txtCampaign != null) // 캠페인 텍스트 있으면
        {
            txtCampaign.color = defaultColor; // 텍스트 기본 색까로 변경
        }

        if (txtChapter != null)
        {
            txtChapter.color = defaultColor;
        }

        if (txtDifficulty != null)
        {
            txtDifficulty.color = defaultColor;
        }

        if (txtCharacter != null)
        {
            txtCharacter.color = defaultColor;
        }
    }

    private void Start()
    {
        if (btnCampaign != null)
        {
            btnCampaign.onClick.AddListener(OpenCampaign);
        }

        if (btnChapter != null)
        {
            btnChapter.onClick.AddListener(OpenChapter);
        }

        if (btnDifficulty != null)
        {
            btnDifficulty.onClick.AddListener(OpenDifficulty);
        }

        if (btnCharacter != null)
        {
            btnCharacter.onClick.AddListener(OpenCharacter);
        }

        if (btnStart != null)
        {
            btnStart.onClick.AddListener(OnClickStart);
        }

        if (btnBack != null)
        {
            btnBack.onClick.AddListener(OnClickBack);
        }

        if (btnWarningClose != null)
        {
            btnWarningClose.onClick.AddListener(CloseWarningPopup);
        }
    }

    private void Update()
    {
        // esc키 눌리면
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 현재 리스트 닫기 함수 호출
            CloseCurrentList();
        }
    }

    // 현재 리스트 닫기 함수
    private void CloseCurrentList()
    {
        // 만약 현재 활성화된 리스트가 없으면
        if (currentActiveList == null)
        {
            return; // 반환
        }

        // 현재 활성화된 리스트 비활성화
        currentActiveList.SetActive(false);
        // 현재 활성화된 리스트는 널
        currentActiveList = null;

    }


    public void OpenCampaign() // 캠페인 열기 함수
    { 
        OpenList(campaignList); // 캠페인 리스트 열기
    }

    public void OpenChapter() // 챕터 열기 함수
    { 
        OpenList(chapterList); // 챕터 리스트 열기

    }
    public void OpenDifficulty() // 난이도 열기 함수
    {
        OpenList(difficultyList); // 난이도 리스트 열기
    }

    public void OpenCharacter() // 캐릭터 열기 함수
    {
        OpenList(characterList); // 캐릭터 리스트 열기
    }

    // 리스트 열기 함수
    private void OpenList(GameObject list)
    {
        // 리스트 닫기 함수 호출
        CloseCurrentList();
        // 리스트 저장
        currentActiveList = list;
        // 저장된 리스트가 있으면
        if (currentActiveList != null)
        {
            // 리스트 활성화
            currentActiveList.SetActive(true);
        }
    }

    // 캠페인 선택 이벤트 함수
    public void OnSelectCampaign(string StageId)
    {
        // UI매니저에 선택한 캠페인 알리기
        UIManager.Instance.RequestSelectCampaign(StageId);

        if (txtCampaign != null) // 택스트 연결되있으면
        {
            txtCampaign.color = selectedColor; // 텍스트 초록색으로
        }

        CloseCurrentList(); // 리스트 닫기
    }

    // 챕터 선택 이벤트 함수
    public void OnSelectChapter(int chapter) 
    { 
        // UI매니저에 선택한 챕터 알리기
        UIManager.Instance.RequestSelectChapter(chapter);

        if (txtChapter != null)
        {
            txtChapter.color = selectedColor;
        }

        CloseCurrentList();
    }

    // 난이도 선택 이벤트 함수
    public void OnSelectDifficulty(int diff) 
    {
        // UI매니저에 선택한 난이도 알리기
        UIManager.Instance.RequestSelectDifficulty(diff);

        if (txtDifficulty != null)
        {
            txtDifficulty.color = selectedColor;
        }

        CloseCurrentList();
    }

    // 캐릭터 선택 이벤트 함수
    public void OnSelectCharacter(int charIndex) 
    { 
        // UI매니저에 선택한 캐릭터 알리기
        UIManager.Instance.RequestSelectCharacter(charIndex);

        if (txtCharacter != null)
        {
            txtCharacter.color = selectedColor;
        }

        CloseCurrentList();
    }

    // 시작 클릭 이벤트
    public void OnClickStart() 
    {
        // UI매니저에 시작버튼 눌렀다고 알리기
        UIManager.Instance.RequestLoading(); 
    }

    public void OnClickBack()  // 뒤로가기 이벤트 함수
    { 
        // UI매니저에서 메인UI 호출
        UIManager.Instance.CallMainUI();
    }

    public void ShowWarningPopup() // 경고창 열기 함수
    {
        if (warningPopup != null)
        {
            warningPopup.SetActive(true);
        }
    }

    private void CloseWarningPopup() // 경고창 닫기 함수
    {
        if (warningPopup != null)
        {
            warningPopup.SetActive(false);
        }
    }
}
