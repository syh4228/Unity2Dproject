using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using System;

public class StartUIManager : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [SerializeField] private Image LoadingBar; // 로딩바 연결
    [SerializeField] private TextMeshProUGUI LoadingText; // 택스트 연결

    // UI매니저에서 호출하는 함수
    public async UniTask StartLoading()
    {
        // 1차 로딩바 연출
        await FillLoadingBar(0f, 0.5f, 1.5f, "세계를 불러오는 중...");

        // 실제 데이터 로드 (추후 여기에 로드 로직 추가 계획)
        LoadingText.text = "기지로 이동 중...";
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f));

        // 2차 로딩바 연출
        await FillLoadingBar(0.5f, 1f, 0.5f, "기지 도착!");

        LoadingText.text = "기지 도착! [엔터를 치면 이동합니다.]";

        while (true)
        {
            // 엔터키 누르면
            if (Input.GetKeyDown(KeyCode.Return))
            {
                break; // 빠져나옴
            }

            // 다음 프레임까지 대기
            await UniTask.Yield();
        }
    }

    
    // 로딩바 연출 함수 (시작점, 도착점, 시간, 메세지)
    private async UniTask FillLoadingBar(float start, float end, float duration, string msg)
    {
        LoadingText.text = msg; // 택스트 저장
        float timer = 0f; // 시간 세기

        while (timer < duration) // 0 보다 지정 시간이 크면
        {
            timer += Time.deltaTime; // 0에 시간 플러스
            // 로딩바 진행 계산
            LoadingBar.fillAmount = Mathf.Lerp(start, end, timer / duration);

            // 다음 프레임까지 대기
            await UniTask.Yield();
        }

        // 로딩바 완료
        LoadingBar.fillAmount = end;
    }
}
