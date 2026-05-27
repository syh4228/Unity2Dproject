using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    [Header("버튼 연결")]
    [SerializeField] private Button playButton; // 로비로 가는 버튼
    [SerializeField] private Button bookButton; // 도감 버튼
    [SerializeField] private Button optionButton; // 설정 버튼
    [SerializeField] private Button exitButton; // 종료 버튼

    [Header("도감 연결")]
    [SerializeField] private GameObject itemBook; // 도감 연결

    private void Start()
    {
        if (playButton != null)
        {
            // 캠페인 버튼 눌림 함수 호출
            playButton.onClick.AddListener(OnPlayClick);
        }

        if (bookButton != null)
        {
            // 도감 버튼 눌림 함수 호출
            bookButton.onClick.AddListener(OnBookClick);
        }

        if (optionButton != null)
        {
            // 설정 버튼 눌림 함수 호출
            optionButton.onClick.AddListener(OnOptionClick);
        }

        if (exitButton != null)
        {
            // 종료 버튼 눌림 함수 호출
            exitButton.onClick.AddListener(OnExitClick);
        }
    }

    private void OnPlayClick()
    {
        // UI매니저에 캠페인 버튼 눌림 알림
        UIManager.Instance.RequestPlayGame();
    }

    private void OnBookClick()
    {
        if (itemBook != null)
        {
            itemBook.SetActive(true);
        }
        else
        {
            UtillLogRemove.Error("도감 연결 확인요망");
        }
    }

    private void OnOptionClick()
    {
        Debug.Log("설정은 추후 구현 예정");
    }

    private void OnExitClick()
    {
        // UI 매니저에게 종료 버튼 눌림 알림
        UIManager.Instance.ExitGameWithUI();
    }
}
