using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public float speed = 10f; // 총알 속도
    public int damage = 20;   // 총알 대미지

    private Rigidbody2D bulletRigidbody; // 총알 리지디바디 받기

    private void Awake()
    {
        // 리지디바디 가져오기
        bulletRigidbody = GetComponent<Rigidbody2D>();
    }

    // 총알 발사 방향 함수
    public void SetDirection(float directionX)
    {
        // 총알 속도 계산
        bulletRigidbody.linearVelocity = new Vector2(directionX * speed, 0f);

        // 총알 발사 위치가 x 보다 작으면
        if (directionX < 0)
        {
            // 총알 방향 뒤집기
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else // 아니면
        {
            // 뒤집지 않음
            GetComponent<SpriteRenderer>().flipX = false;
        }
    }

    // 총알이 적과 붙이쳤을떄 함수
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 부딪힌 대상의 태그가 "Enemy" 라면
        if (collision.CompareTag("Enemy"))
        {
            // 적에게 붙어있는 스탯 매니저를 받기
            Enemy_StatManager enemyStat = collision.GetComponent<Enemy_StatManager>();

            if (enemyStat != null) // 스탯 매니저를 받았으면
            {
                enemyStat.currentHp -= damage; // 체력 감소

                if (enemyStat.currentHp < 0) // 현재체력이 0보다 작으면
                {
                    enemyStat.currentHp = 0; // 현재체력 0
                }

                UtillLogRemove.Log($"적에게 대미지 {damage}를 주었습니다! 남은 체력: {enemyStat.currentHp}");
            }

            gameObject.DeactivateSafe(); // 총알 비활성화
        }

        // 만약 땅 타일맵(Default 레이어 등)에 부딪혀도 총알 파괴
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            gameObject.DeactivateSafe(); // 총알 비활성화
        }
    }
}
