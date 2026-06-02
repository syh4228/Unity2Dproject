using UnityEngine;

public class EnemySkill_Bomber : MonoBehaviour
{
    [Header("자폭 세팅")]
    public float explosionRadius = 3f; // 자폭 범위
    public float alertRadius = 15f; // 주변 좀비 어그로 범위

    // 자폭 실행 함수
    public void ExecuteExplosion(Transform player, Enemy_StatManager statManager)
    {
        UtillLogRemove.Log("바머 좀비 폭발");

        // 플레이어가 자폭범위 안에 있으면
        if (Vector2.Distance(transform.position, player.position) <= explosionRadius)
        {
            // 플레이어 컴포넌트 가져와 저장
            Player_Character playerCharacter = player.GetComponent<Player_Character>();

            if (playerCharacter != null) // 플레이어 캐릭터 있으면
            {
                // 스텟매니저에서 공격력 가져와 저장
                int finalDamage = statManager.Attack;

                // 플레이어에게 데미지 전달
                BattleManager.Instance.ProcessEnemyAttack(playerCharacter, finalDamage);

                UtillLogRemove.Log("자폭 데미지: " + finalDamage);
            }
        }

        // 좀비 강제 웨이브 함수 호출
        TriggerSuicideWave(player);
        // 바머 사망 처리
        statManager.TakeDamage(99999);
    }

    // 좀비 강제 웨이브 함수
    private void TriggerSuicideWave(Transform player)
    {
        UtillLogRemove.Log("강제 좀비 웨이브 발생");

        // 주변 좀비 어그로 반경 내의 모든 콜라이더 배열로 저장
        Collider2D[] surroundingZombies = Physics2D.OverlapCircleAll(transform.position, alertRadius);

        foreach (Collider2D col in surroundingZombies) // 하나씩 꺼내서 저장
        {
            // 태그가 적이고, 내가 아니면
            if (col.CompareTag("Enemy") == true && col.gameObject != this.gameObject)
            {
                // 적 Ai매니저에서 컴포넌트 가져와 저장
                Enemy_AiManager ai = col.GetComponent<Enemy_AiManager>();

                if (ai != null) // 적 ai가 있으면
                {
                    // 감지범위 강제로 99로 증가
                    ai.detectRange = 99f;
                }
            }
        }
    }
}
