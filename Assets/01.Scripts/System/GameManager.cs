using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // 싱글턴 선언

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

    private void Update()
    {
        // ESC키 누르면
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame(); // 게임종료
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
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

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

    // 게임종료 함수
    public void QuitGame() 
    {
        UtillLogRemove.Log("게임 종료 실행됨");

        Application.Quit(); // 실제 종료 실행
    }
}
