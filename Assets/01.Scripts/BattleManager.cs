using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance; // 싱글턴 선언

    [Header("게임 상태 변수")]
    private int score = 0; // 좀비 잡은 수
    private bool isGameOver = false; // 게임오버인지 확인

    private void Awake()
    {
        if (Instance == null) // 만약 인스턴스가 널이면
        {
            Instance = this; // 인스턴스는 자기자신
        }
        else // 아니면
        {
            Object.Destroy(gameObject); // 게임오브젝트 삭제
        }
    }

    private void Start()
    {
        if (UIManager.Instance != null) // UI매니저 인스턴스가 있으면
        {
            UIManager.Instance.UpdateScoreUI(score); // UI 매니저 스코어 점수 함수 호출
        }
    }

    // 플레이어가 총으로 적을 맞췄을 때 호출되는 함수
    public void ProcessPlayerAttack(Enemy_StatManager enemy, int baseBulletDamage)
    {
        if (enemy == null || isGameOver == true) // 만약 적이 없고, 게임오버됬으면
        {
            return; // 반환
        }

        int finalDamage = baseBulletDamage; // 대미지 계산

        enemy.TakeDamage(finalDamage); // 적 대미지 적용 함수 호출
    }

    // 적이 플레이어를 공격 했을 때 호출되는 함수
    public void ProcessEnemyAttack(Player_Character player, int enemyBaseAttack)
    {
        if (player == null || isGameOver == true)
        {
            return;
        }

        int finalDamage = enemyBaseAttack; // 대미지 계산

        player.TakeDamage(finalDamage); // 플레이어 대미지 적용 함수 호출
    }

    // 적 처지 및 점수 보상 지급 함수
    public void OnEnemyDefeated(int scoreReward)
    {
        score = score + scoreReward; // 점수 계산

        UtillLogRemove.Warning($"적 처치! 현재 점수: {score}");

        // 만약 UI매니저 인스턴스가 있으면
        if (UIManager.Instance != null)
        {
            // UI매니저 인스턴스 점수 업데이트 함수 호출
            UIManager.Instance.UpdateScoreUI(score);
        }
    }

    // 플레이어 사망 함수
    public void OnPlayerDead()
    {
        if (isGameOver == true) // 만약 게임 오버라면
        {
            return; // 반환
        }

        isGameOver = true; // 게임오버 처리

        UtillLogRemove.Error("GAME OVER");
    }
}
