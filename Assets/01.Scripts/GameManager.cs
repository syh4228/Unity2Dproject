using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // 싱글턴 선언

    [Header("게임 설정")]
    [SerializeField] private float totalGameTime = 120f; // 제한 시간

    private int currentScore = 0; // 현재 점수
    private int highScore = 0; // 최고 점수
    private float timeRemaining; // 타이머
    private bool isGameOver = false; // 게임오버 관리

    public bool IsGameOver // 게임오버 알리기
    {
        get
        {
            return isGameOver;
        }
    }

    private void Awake()
    {
        if (Instance == null) // 만약 인스턴스가 없으면
        {
            Instance = this; // 자체 인스턴스 설정
        }
        else
        {
            Object.Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 최고 점수 저장
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        timeRemaining = totalGameTime; // 시간 저장

        UIManager.Instance.UpdateScore(currentScore); // 점수 업데이트 함수 호출
        UIManager.Instance.UpdateHighScore(highScore); // 최고 점수 업데이트 함수 호출
        UIManager.Instance.UpdateTimer(timeRemaining); // 타이머 업데이트 함수 호출
    }

    private void Update()
    {
        if (isGameOver) // 게임오버 상태면
        {
            return; // 반환
        }

        if (timeRemaining > 0) // 시간이 0보다 크면
        {
            timeRemaining -= Time.deltaTime; // 시간 흐르기
            UIManager.Instance.UpdateTimer(timeRemaining); // 타이어 함수 호출
        }
        else // 아니면
        {
            timeRemaining = 0; // 시간이 0초면
            UIManager.Instance.UpdateTimer(timeRemaining); // 타이머 함수 호출
            GameOver(); // 게임오버 함수 호출
        }
    }
    
    // 점수 추가 함수
    public void AddScore(int amount)
    {
        if (isGameOver) // 게임오버면
        {
            return; // 반환
        }

        currentScore += amount; // 현재 점수 저장
        UIManager.Instance.UpdateScore(currentScore); // 점수 함수 호출

        // 만약 현재 점수가 최고 점수보다 크면
        if (currentScore > highScore)
        {
            highScore = currentScore; // 최고점수로 저장
            PlayerPrefs.SetInt("HighScore", highScore); // 유니티 저장
            UIManager.Instance.UpdateHighScore(highScore); // 최고점수 함수 호출
        }
    }

    // 게임오버 함수
    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;

        // 게임오버 UI 호출
        UIManager.Instance.ShowGameOver();

        // 팁: 게임오버 시 적 스폰을 멈추거나 플레이어 조작을 막는 로직을 여기에 추가하면 됩니다.
    }

    // 재시작 함수
    public void RestartGame()
    {
        Time.timeScale = 1f; // 게임오버에 멈춘 시간 다시 흐르기

        // 액션 씬 가져오기
        Scene currentScene = SceneManager.GetActiveScene();
        // 씬 호출
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
