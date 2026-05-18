using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; } // 싱글턴 선언

    [Header("UI 컴포넌트")]
    [SerializeField] private GameObject mainUI; // 메인 UI
    [SerializeField] private GameObject gameOverCanvas; // 게임오버 UI
    [SerializeField] private BattleUIManager battleUI; // 배틀UI

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

    public void ResetUI() // 시작시 UI 함수
    {
        if (mainUI != null) // 메인 UI가 있으면
        {
            mainUI.SetActive(true); // 활성화
        }
        if (gameOverCanvas != null) // 게임오버 UI가 있으면
        {
            gameOverCanvas.SetActive(false); // 비활성화
        }
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
