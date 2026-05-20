using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

public class StartUIManager : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [SerializeField] private Image LoadingBar; // 로딩바 연결
    [SerializeField] private TextMeshProUGUI LoadingText; // 택스트 연결

    // UI매니저에서 호출하면 시작하는 함수
    public void StartLoading(Action onComplete)
    {
        StartCoroutine(LoadRoutine(onComplete));
    }

    // 코루틴을 이용한 로딩바 출력 및 데이터 로드 함수
    private IEnumerator LoadRoutine(Action onComplete)
    {
        // 1차 로딩바 연출 (0% -> 50%)
        yield return StartCoroutine(FillLoadingBar(0f, 0.5f, 1.5f, "세계를 불러오는 중..."));

        // 실제 데이터 로드 구간
        LoadingText.text = "기지로 이동 중...";
        yield return new WaitForSeconds(1.0f); // 추후 로드 코드 관련 추가 예정

        // 2차 로딩바 연출 (50% -> 100%)
        yield return StartCoroutine(FillLoadingBar(0.5f, 1f, 0.5f, "기지 도착!"));

        // 택스트 출력
        LoadingText.text = "기지 도착! [엔터를 치면 이동합니다.]";

        while (true)
        {
            // 만약 엔터를 눌렀다면
            if (Input.GetKeyDown(KeyCode.Return))
            {
                break;
            }
            
            // 엔터 안누르면 무한 반복
            yield return null;
        }

        if (onComplete != null) // 콜백 연결 되있으면
        {
            onComplete.Invoke(); // 콜백 실행
        }
    }

    // 로딩바 연출 함수 (시작점, 도착점, 시간, 메세지)
    private IEnumerator FillLoadingBar(float start, float end, float duration, string msg)
    {
        LoadingText.text = msg; // 택스트 저장
        float timer = 0f; // 시간 세기
        while (timer < duration) // 0 보다 지정 시간이 크면
        {
            timer += Time.deltaTime; // 0에 시간 플러스
            // 로딩바 진행 계산
            LoadingBar.fillAmount = Mathf.Lerp(start, end, timer / duration);
            yield return null; // 반복
        }
        LoadingBar.fillAmount = end;
    }
}
