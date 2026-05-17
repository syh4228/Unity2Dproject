using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; } // 싱글턴 선언

    [Header("UI 컴포넌트")]
    [SerializeField] private GameObject mainUI; // 메인 UI
    [SerializeField] private GameObject gameOverCanvas; // 게임오버 UI

    [Header("UI 텍스트")]
    [SerializeField] private TMP_Text scoreText; // 스코어 점수
    [SerializeField] private TMP_Text highScoreText; // 최고 점수
    [SerializeField] private TMP_Text timerText; // 타이머

    private void Awake()
    {
        if (Instance == null) // 싱글턴이 없으면
        {
            Instance = this; // 자체 싱글턴 

            DontDestroyOnLoad(gameObject);
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
            gameOverCanvas.SetActive(true); // 비활성화
        }
    }

    // 스코어 관리 함수
    public void UpdateScore(int score)
    {
        if (scoreText != null) // 스코어택스트 있으면
        {
            scoreText.text = $"점수 {score}"; // 점수 받아오기
        }
    }

    // 최고 점수 관리 함수
    public void UpdateHighScore(int highScore)
    {
        if (highScoreText != null) // 만약 최고점수 택스트가 있으면
        {
            // 최고 점수 받아오기
            highScoreText.text = $"최고 점수 {highScore}";
        }
    }

    // 타이머 관리
    public void UpdateTimer(float time)
    {
        if (timerText != null) // 타이머 택스트 있으면
        {
            // 남은 시간 받아오기
            timerText.text = $"남은 시간 {Mathf.CeilToInt(time)}";
        }
    }
}
