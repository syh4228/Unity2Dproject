using UnityEngine;

public class WaitingRoomUIManager : MonoBehaviour
{
    [Header("리스트 연결")]
    [SerializeField] private GameObject campaignList; // 캠페인
    [SerializeField] private GameObject chapterList; // 챕터
    [SerializeField] private GameObject difficultyList; // 난이도
    [SerializeField] private GameObject characterList; // 캐릭터

    private GameObject currentActiveList; // 현재 활성화된 리스트 저장

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
        if (currentActiveList != null)
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
        CloseCurrentList(); // 리스트 닫기
    }

    // 챕터 선택 이벤트 함수
    public void OnSelectChapter(int chapter) 
    { 
        // UI매니저에 선택한 챕터 알리기
        UIManager.Instance.RequestSelectChapter(chapter); 
        CloseCurrentList();
    }

    // 난이도 선택 이벤트 함수
    public void OnSelectDifficulty(int diff) 
    {
        // UI매니저에 선택한 난이도 알리기
        UIManager.Instance.RequestSelectDifficulty(diff);
        CloseCurrentList();
    }

    // 캐릭터 선택 이벤트 함수
    public void OnSelectCharacter(string charId) 
    { 
        // UI매니저에 선택한 캐릭터 알리기
        UIManager.Instance.RequestSelectCharacter(charId);
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
}
