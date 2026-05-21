using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; } // 싱글턴 선언

    [Header("정보전달 컴포넌트")]
    [SerializeField] private StartUIManager startUIManager; //  스타트 UI
    [SerializeField] private BattleUIManager battleUIManager; // 배틀UI
    [SerializeField] private LodingUIManager loadingUIManager; // 로딩 UI
    [SerializeField] private WaitingRoomUIManager lobbyUIManager; // 로비 UI

    [Header("연결 컴포넌트")]
    [SerializeField] private GameObject startUI; //스타트 UI
    [SerializeField] private GameObject mainUI; // 메인 UI
    [SerializeField] private GameObject LobbyUI; // 로비 UI
    [SerializeField] private GameObject LoadingUI; // 로딩 UI
    [SerializeField] private GameObject battleUI; // 배틀UI
    [SerializeField] private GameObject scoreUI; // 점수UI

    // 스타트UI 매니저 정보를 주는 함수
    public StartUIManager GetStartUI()
    {
        return startUIManager;
    }

    public BattleUIManager GetBattleUI()
    {
        return battleUIManager;
    }

    public LodingUIManager GetLodingUI()
    {
        return loadingUIManager;
    }


    private void Awake()
    {
        if (Instance == null) // 싱글턴이 없으면
        {
            Instance = this; // 자체 싱글턴 
        }
        else
        {
            gameObject.DestroySafe();
        }
    }

    public void Allcolse() // UI 전체 비활성화 함수
    {
        if (startUI  != null)
        {
            startUI.SetActive(false);
        }

        if (mainUI != null)
        {
            mainUI.SetActive(false);
        }

        if (LobbyUI != null)
        {
            LobbyUI.SetActive(false);
        }

        if (LoadingUI != null)
        {
            LoadingUI.SetActive(false);
        }

        if (battleUI != null)
        {
            battleUI.SetActive(false);
        }

        if (scoreUI != null)
        {
            scoreUI.SetActive(false);
        }
    }

    public void CallStartUI() // 스타트 UI 호출 함수
    {
        Allcolse(); // 전체 한번 비활성화

        if (startUI != null)
        {
            startUI.SetActive(true);
        }
    }

    public void CallMainUI()
    {
        Allcolse();
       
        if (mainUI != null)
        {
            mainUI.SetActive(true);
        }
    }

    public void CallLobbyUI()
    {
        Allcolse();

        if (LobbyUI != null)
        {
            LobbyUI.SetActive(true);
        }
    }

    public void CallLoadingUI()
    {
        Allcolse();

        if (LoadingUI != null)
        {
            LoadingUI.SetActive(true);
        }
    }

    public void CallBattleUI()
    {
        Allcolse();

        if (battleUI != null)
        {
            battleUI.SetActive(true);
        }
    }

    public void CallscoreUI()
    {
        Allcolse();

        if (scoreUI != null)
        {
            scoreUI.SetActive(true);
        }
    }

    public void RequestPlayGame() // 캠페인버튼 이벤트 함수
    {
        Allcolse(); //  모든 UI 닫기 함수 호출

        // 게임 매니저에 캠페인 버튼 눌렸다 알림
        GameManager.Instance.BtnClick_Play();
    }

    public void ExitGameWithUI() // 종료버튼 이벤트 함수
    {
        Allcolse(); // 모든 UI 닫기 함수 호출

        // 게임 매니저에게 종료 버튼 눌렸다 알림
        GameManager.Instance.BtnClick_ExitGame();
    }

    // 선택 캠패인 정보 넘겨주기 함수
    public void RequestSelectCampaign(string StageId)
    {
        // 게임매니저에 선택 캠페인 알림
        GameManager.Instance.SelectCampaign(StageId);
    }

    // 선택 챕터 정보 넘겨주기 함수
    public void RequestSelectChapter(int chapter) 
    { 
        // 게임매니저에 선택 챕터 알림
        GameManager.Instance.SelectChapter(chapter); 
    }

    // 선택 난이도 정보 넘겨주기 함수
    public void RequestSelectDifficulty(int diff)
    {
        // 게임매니저에 선택 난이도 알림
        GameManager.Instance.SetDifficulty(diff); 
    }

    // 선택 캐릭터 정보 넘겨주기 함수
    public void RequestSelectCharacter(int charIndex)
    {
        // 게임매니저에 선택 캐릭터 알림
        GameManager.Instance.SetCharacter(charIndex);
    }

    // 로딩UI로 호출 함수
    public void RequestLoading()
    {
        // 게임매니저에 로딩UI로 게임 시작 알림
        GameManager.Instance.BtnClick_GameStart();
    }

    // 나가기 팝업 호출 함수
    public void ShowExitpopup(bool isActive)
    {
        if (battleUIManager != null) // 배틀UI 매니저가 있으면
        {
            // 배틀UI 매니저에서 나가기 함수 호출
            battleUIManager.ShowExitPopup(isActive);
        }
    }

    // 나가기 팝업 켜져있는지 묻는 함수
    public bool IsExitPopupActive()
    {
        // 배틀UI 매니저가 있으면
        if (battleUIManager != null)
        {
            // 배틀매니저에 나가기 팝업 켜져있는지 확인하는 함수 호출
            return battleUIManager.IsExitPopupActive();
        }

        return false;
    }

    // 나가기 버튼 이벤트 알림 함수
    public void RequestExitBattle()
    {
        // 게임매니저에게 게임상태 로비로 바꿔야한다고 알림
        GameManager.Instance.ChangeState(GameState.Lobby);   
    }

    public void ShowLobbyWarning() // 경고창 이벤트 알림 함수
    {
        if (lobbyUIManager != null) // 로비UI매니저가 있으면
        {
            // 경고창 열기함수 호출
            lobbyUIManager.ShowWarningPopup();
        }
    }
}
