using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public float speed = 10f; // 총알 속도
    public int damage = 20;   // 총알 대미지

    public DamageType bulletDamageType; // 대미지 타입 저장 변수

    private Rigidbody2D bulletRigidbody; // 총알 리지디바디 받기
    private SpriteRenderer bulletSpriteRenderer; // 총알 스프라이트 받기

    private float _effectiveRange = 0f;  // 최대 사거리
    private Vector2 _startPosition;      // 발사 위치

    private void Awake()
    {
        // 리지디바디 가져오기
        bulletRigidbody = GetComponent<Rigidbody2D>();
        // 총알 스프라이트 받기
        bulletSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // 총알 발사 방향 함수
    public void SetDirection(float directionX, int weaponDamage, DamageType weaponType, float effectiveRange)
    {
        damage = weaponDamage; // 무기 대미지 저장
        bulletDamageType = weaponType; // 무기 타입 저장
        _effectiveRange = effectiveRange; // 최대 사거리 저장
        _startPosition = transform.position; // 발사 위치 저장

        if (bulletRigidbody == null) // 리지드바디 없으면
        {
            //리지드바디 가져오기
            bulletRigidbody = GetComponent<Rigidbody2D>();
        }

        if (bulletRigidbody != null)  // 리지드바디 있으면
        {
            // 총알 속도 계산
            bulletRigidbody.linearVelocity = new Vector2(directionX * speed, 0f);
        }

        if (bulletSpriteRenderer != null) // 스프라인트 있으면
        {
            // directionX가 0보다 작으면(왼쪽) true, 아니면 flase
            bulletSpriteRenderer.flipX = (directionX < 0);
        }
        else // 아니면
        {
            // 자식에서 스프라이트 가져오기
            bulletSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            //  스프라이트 있으면
            if (bulletSpriteRenderer != null)
            {   
                bulletSpriteRenderer.flipX = (directionX < 0);
            }
        }
    }

    private void Update()
    {
        // 총알 현재 위치와 출발위치 저장
        float traveledDistance = Vector2.Distance(_startPosition, transform.position);

        // 위치가 사거리보다 크면
        if (traveledDistance >= _effectiveRange)
        {
            UtillLogRemove.Log("총알이 최대 사거리에 도달하여 비활성화됩니다.");
            gameObject.DeactivateSafe(); // 총알 비활성화
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
                // 배틀매니저 인스턴스가 있으면
                if (BattleManager.Instance != null) 
                {
                    // 배틀매니저에서 함수 호출
                    BattleManager.Instance.ProcessPlayerAttack(enemyStat, damage, bulletDamageType);
                }
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
