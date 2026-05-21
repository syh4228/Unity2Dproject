using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;


public class LodingUIManager : MonoBehaviour
{
    [Header("로딩바 컴포넌트")]
    [SerializeField] private Image loadingBar; //  로딩바 이미지
    [SerializeField] private TextMeshProUGUI loadingText; // 로딩시 사용할 텍스트 

    [Header("로딩 설정")]
    [SerializeField] private float loadingbartimer = 2.0f; // 로딩바 속도
    [SerializeField] private string IookLodingText = "이동 중"; // 로딩바 텍스트에 실제 출력 되는 텍스트

    // UniTask 멈추게 하기위한 변수저장
    private CancellationTokenSource cts;

    private bool isDataLoad = false; // 맵과 플레이어 생성이 끝났는지 확인 위한 변수

    private void OnEnable()
    {
        if (loadingBar != null) // 로딩바 있으면
        {
            // 로딩바 0에서 시작
            loadingBar.fillAmount = 0f;
        }

        isDataLoad = false; // 로딩UI 새로 킬때마다 초기화

        cts = new CancellationTokenSource();

        // 로딩바 채우기 함수 호출
        FillBarRoutineAsync(cts.Token).Forget();
        // 텍스트 애니메이션 함수 호출
        TextAnimationRoutineAsync(cts.Token).Forget();
    }

    private void OnDisable()
    {
        if (cts != null) // 널이 아니면
        {
            cts.Cancel(); // 취소
            cts.Dispose(); // 처분
            cts = null; // 널 전환
        }
    }

    private async UniTaskVoid FillBarRoutineAsync(CancellationToken Token)
    {
        float timer = 0f; // 시간 = 0

        while (timer < loadingbartimer) // 로딩 타이머가 시간 보다 크면
        {
            timer += Time.deltaTime; // 시간 +

            if (loadingBar != null) // 로딩바가 있으면
            {
                // 로딩바 채우기 계산
                loadingBar.fillAmount = Mathf.Lerp(0f, 0.9f, timer / loadingbartimer);
            }

            // 대기
            await UniTask.Yield(PlayerLoopTiming.Update, Token);
        }

        while (isDataLoad == false) // 데이터다운이 다 안됬으면
        {
            // 무한 대기
            await UniTask.Yield(PlayerLoopTiming.Update, Token);
        }

        if (loadingBar != null) // 로딩바 있으면
        {
            // 로딩바 채우기
            loadingBar.fillAmount = 1f;
        }
    }

    // 택스트 애니메이션 함수
    private async UniTaskVoid TextAnimationRoutineAsync(CancellationToken token)
    {
        int dotCount = 0; // 점 카운트 = 0

        while (!token.IsCancellationRequested) // 토큰 정지 명령이 없으면
        {
            if (loadingText != null) // 로딩 텍스트 있으면
            {
                // 점이 3개 까지만 나오는 계산 
                string dots = new string(' ', dotCount % 4);
                // 화면에 텍스트 + 점 출력
                loadingText.text = IookLodingText + dots;
            }

            dotCount++; // 점 카운트 증가
            // 깜박임 대기 시간
            await UniTask.Delay(TimeSpan.FromSeconds(0.4f), cancellationToken: token);
        }
    }

    public void SetDataLoaded()
    {
        isDataLoad = true;
    }
}
