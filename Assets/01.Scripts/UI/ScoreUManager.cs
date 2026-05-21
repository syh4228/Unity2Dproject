using UnityEngine;
using TMPro;

public class ScoreUManager : MonoBehaviour
{
    [Header("텍스트 연결")]
    [SerializeField] private TextMeshProUGUI killScoreText; // 킬 수 로그
    [SerializeField] private TextMeshProUGUI receivedDamageText; // 받은 피해량 로그
    [SerializeField] private TextMeshProUGUI givenDamageText; // 적에게 가한 피해량 로그
    [SerializeField] private TextMeshProUGUI recoveredText; // 회복탬 사용 횟수 로그
    [SerializeField] private TextMeshProUGUI timeText; // 플레이 시간 로그

    // 스코어데이터 출력함수
    public void SetScoreDate(int kill, float rdDmg, float gnDmg, int heelCount, float PlayTime)
    {
        if (killScoreText != null)
        {
            killScoreText.text = ($"처치한 적: {kill}");
        }

        if (receivedDamageText != null)
        {
            receivedDamageText.text = ($"받은 피해량: {rdDmg}");
        }

        if ( givenDamageText != null)
        {
            givenDamageText.text = ($"적에게 준 피해량: {gnDmg}");
        }

        if ( recoveredText != null)
        {
            recoveredText.text = ($"사용한 회복탬: {heelCount}개");
        }

        // 시간계산
        int minutes = Mathf.FloorToInt(PlayTime / 60);
        int seconds = Mathf.FloorToInt(PlayTime  % 60);


        if (timeText != null)
        {
            timeText.text = string.Format($"플레이 시간: {0:00}:{1:00}", minutes, seconds);
        }
    }
}
