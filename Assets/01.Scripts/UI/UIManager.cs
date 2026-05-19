using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; } // 싱글턴 선언

    [Header("UI 컴포넌트")]
    [SerializeField] private GameObject mainUI; // 메인 UI
    [SerializeField] private GameObject gameOverCanvas; // 게임오버 UI
    [SerializeField] private BattleUIManager battleUI; // 배틀UI
    [SerializeField] private GameObject startUI; //  스타트 UI
    [SerializeField] private LoadingManager loadingManager; // 스타트 UI 로딩 연출

    public BattleUIManager GetBattleUI() // 배틀UI 주는 함수
    {
        return battleUI; // 배틀UI 반환
    }

    private void Awake()
    {
        if (Instance == null) // 싱글턴이 없으면
        {
            Instance = this; // 자체 싱글턴 
        }
        else
        {
            Object.Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetUI(); // 시작시 UI 함수 호출
    }

    public void ResetUI() // 시작시 UI 리셋 함수
    {
        if (startUI != null) // 만약  스타트 UI 있으면
        {
            startUI.SetActive(true); // 스타트 UI 활성화
        }
        if (mainUI != null) // 메인 UI가 있으면
        {
            mainUI.SetActive(false); // 비 활성화
        }
        if (gameOverCanvas != null) // 게임오버 UI가 있으면
        {
            gameOverCanvas.SetActive(false); // 비활성화
        }
    }

    // 스타트 UI 호출 함수
    public void showStartUI()
    {
        if (loadingManager != null) // 로딩 매니저가 있으면
        {
            // 로딩매니저 스타트 로딩 함수 호출
            loadingManager.StartLoading(OnLoadingComplete);
        }
    }

    // 스타트 UI 끝나면 메인 UI로 전환하는 함수
    private void OnLoadingComplete()
    {
        if (startUI != null) // 만약 스타트UI 가 있으면
        {
            startUI.SetActive(false); // 비활성화
        }

        if (mainUI != null) // 만약 메인 UI 가 있으면
        {
            mainUI.SetActive(true); // 메인 UI 활성화
        }

        Debug.Log("로딩 종료 -> 메인 UI 전환 완료 (정석 함수 실행됨)");
    }

    // 게임오버 UI 호출 함수
    public void ShowGameOver()
    {
        if (mainUI != null) // 메인UI 있으면
        {
            mainUI.SetActive(false); // 비활성화
        }
        if (gameOverCanvas != null) // 게임오버 UI 있으면
        {
            gameOverCanvas.SetActive(true); // 활성화
        }
    }
}
