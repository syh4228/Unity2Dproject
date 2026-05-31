using UnityEngine;

public class Player_Melee : MonoBehaviour
{
    [Header("근접 공격 설정")]
    public int MeleeDamage = 100; // 데미지
    public float AttackRadius = 1.2f; // 공격 반경
    public Transform AttackPoint; // 플레이어가 보는 방향

    [SerializeField] private AnimationController animController; // 애니메이션 컨트롤러 연결

    // 근접공격 실행 함수
    public void ExecuteMelee(bool isFaceRight)
    {
        if (animController != null) // 애니메이션 컨트로 있으면
        {
            // 애니메이션 컨트롤러 상태 근접공격으로 변경
            animController.SetState(AllState.Melee);
        }

        // 플레이어가 보는 방향에 공격 반경에 있는 적 레이어를 가진 대상 저장
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(AttackPoint.position, AttackRadius, LayerMask.GetMask("Enemy"));

        // 배열에서 하나씩 꺼내서 hit으로 저장
        foreach (Collider2D hit in hitEnemies)
        {
            // 적 스탯 매니저에서 컴포넌트 받아와서 저장
            Enemy_StatManager enemyStat = hit.GetComponent<Enemy_StatManager>();

            // 적 스탯과 배틀매니저 인스턴스가 있으면
            if (enemyStat != null && BattleManager.Instance != null)
            {
                UtillLogRemove.Log("근접 공격 실행");

                // 배틀매니저에 플레이어 공격 함수에 적 맞은 적 정보, 근접 공격 대미지, 공격타입이 근접이라고 전달
                BattleManager.Instance.ProcessPlayerAttack(enemyStat, MeleeDamage, DamageType.Melee);
            }
        }
    }

    // 공격 범위 기즈모로 그리는 함수
    private void OnDrawGizmosSelected()
    {
        if (AttackPoint != null)
        {
            Gizmos.color = Color.blue; // 색깔 파랑
            // 동그란 기즈모원 그리기(플레이어 보는 방향과 공격 범위)
            Gizmos.DrawWireSphere(AttackPoint.position, AttackRadius);
        }
    }
}
