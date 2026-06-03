using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 5f; // 투사체 속도
    private int damage = 0; // 투사체 데미지
    private Vector2 moveDirection; // 투사체 방향

    private float lifeTimer = 0f; // 투사체 생존 시간

    // 투사체 초기화 함수
    public void Initialize(Vector2 direction, int throwerDamage)
    {
        moveDirection = direction.normalized; // 방향 저장
        damage = throwerDamage; // 데미지 저장
        lifeTimer = 0f; // 시간 초기화

        // 목표 각도 계산 저장
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        // 날아가는 방향으로 이미지 회전
        transform.rotation = Quaternion.Euler(0, 0 , angle);
    }

    private void Update()
    {
        // 투사체 목표로 날아가기
        transform.Translate(moveDirection *  speed * Time.deltaTime, Space.World);

        lifeTimer += Time.deltaTime; // 시간 증가
        if (lifeTimer >= 5f) // 시간이 5초 이상이면 
        {
            gameObject.DeactivateSafe(); // 비활성화
            lifeTimer = 0f; // 시간 초기화
        }
    }

    // 투사체 트리거 함수
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 부딪힌 타겟이 플레이어가 맞으면
        if (collision.CompareTag("Player") == true)
        {
            // 플레이어 캐릭터에서 콜라이더 컴포넌트 가져와 저장
            Player_Character player = collision.GetComponent<Player_Character>();

            // 플레이어가 있고, 배틀매니저 인스턴스가 있으면
            if (player != null && BattleManager.Instance != null)
            {
                // 배틀매니저에 적 공격 함수 호풀
                BattleManager.Instance.ProcessEnemyAttack(player, damage);
            }

            gameObject.DeactivateSafe(); // 비 활성화
        }

        else if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            gameObject.DeactivateSafe(); // 비활성화
        }
    }
}
