using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; } // 싱글턴 선언

    [Header("UI 컴포넌트")]
    [SerializeField] private StartUIManager startUIManager; //  스타트 UI 외부로 정보 주기
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
}
