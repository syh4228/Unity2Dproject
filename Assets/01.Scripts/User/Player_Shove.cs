using UnityEngine;

public class Player_Shove : MonoBehaviour
{
    [Header("밀치기 설정")]
    public float ShoveRadius = 1.5f; // 밀치기 범위
    public float ShoveForce = 10f; // 밀어내는 힘
    public float StunTime = 1.5f;  // 경직 시간
    public Transform ShovePoint;   // 플레이어가 보는 반향

    [SerializeField] private AnimationController animController; // 애니메이션 컨트롤러 연결

    // 밀치기 실행 함수
    public void ExecuteShove(bool isFaceRight)
    {
        // 애니메이션 컨트롤러 있으면
        if (animController != null)
        {
            // 애니메이션 컨트롤러 상태 밀치기로 변경
            animController.SetState(AllState.Shove);
        }

        // 플레이어 바라보는 방향에 밀치기 범위에 있는 적 레이어를 가지고 있으면 저장
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(ShovePoint.position, ShoveRadius, LayerMask.GetMask("Enemy"));

        // 바라보고 있는 방향이 오른쪽인지, 왼쪽인지 저장
        Vector2 pushDirection = isFaceRight ? Vector2.right : Vector2.left;

        // 하나씩 꺼내서 저장
        foreach (Collider2D hit in hitEnemies)
        {
            // 적 Ai 매니저에 컴포넌트 받아서 저장
            Enemy_AiManager enemyAi = hit.GetComponent<Enemy_AiManager>();

            if (enemyAi != null)
            {
                UtillLogRemove.Log("밀치기를 실행");
                
                // 적 밀쳐진 함수 실행
                enemyAi.ApplyShove(pushDirection, ShoveForce, StunTime);
            }
        }
    }

    // 적이 밀쳐지는 범위 기즈모로 그리는 함수
    private void OnDrawGizmosSelected()
    {
        if(ShovePoint != null) // 플레이어가 바라보는 방향이 있으면
        {
            Gizmos.color = Color.yellow; // 노랑색
            // 기즈모 구로 그리기
            Gizmos.DrawWireSphere(ShovePoint.position, ShoveRadius);
        }
    }
}
