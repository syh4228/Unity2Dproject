using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance; // 싱글턴 선언

    private void Awake()
    {
        if (Instance == null) // 만약 인스턴스가 널이면
        {
            Instance = this; // 인스턴스는 자기자신
        }
        else // 아니면
        {
            Destroy(gameObject); // 게임오브젝트 삭제
        }
    }

    // 플레이어가 총으로 적을 맞췄을 때 호출되는 함수
    public void ProcessPlayerAttack(Enemy_StatManager enemy, int baseBulletDamage)
    {
        // 적이 없고
        if (enemy == null)
        {
            // 게임매니저가 있고, 게임매니저가 게임오버면
            if (GameManager.Instance != null && GameManager.Instance.IsBattleActive == false)
            {
                return; // 반환
            } 
        }

        int finalDamage = baseBulletDamage; // 대미지 계산
        enemy.TakeDamage(finalDamage); // 적 대미지 적용 함수 호출
    }

    // 적이 플레이어를 공격 했을 때 호출되는 함수
    public void ProcessEnemyAttack(Player_Character player, int enemyBaseAttack)
    {
        // GameManager가 있고, 게임오버 상태라면 공격 무효화
        if (player == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsBattleActive == false)
            {
                return;
            }
        }
        int finalDamage = enemyBaseAttack; // 대미지 계산
        player.TakeDamage(finalDamage); // 플레이어 대미지 적용 함수 호출
    }

    // 플레이어 사망 함수
   public void OnPlayerDead()
    {
        if (GameManager.Instance != null) // 게임 매니저 있으면
        {
            // 게임매니저에 게임오버 호출
           // GameManager.Instance.GameOver();
        }

        UtillLogRemove.Error("GAME OVER");
    } 
}
