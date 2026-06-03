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

    private int _pierceCount = 0; // 저격총 관통 횟수 저장

    private void Awake()
    {
        // 리지디바디 가져오기
        bulletRigidbody = GetComponent<Rigidbody2D>();
        // 총알 스프라이트 받기
        bulletSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // 총알 발사 방향 함수
    public void SetDirection(float dirX, WeaponData gunData)
    {
        _startPosition = transform.position; // 발사 위치 저장
        damage = gunData.Damage; // 무기대미지 저장
        _effectiveRange = gunData.EffectiveRange / 10f; // 무기 사거리 저장

        _pierceCount = 0; // 관통 횟수 초기화

        // 대미지타입 노멀건으로 저장
        bulletDamageType = DamageType.NormalGun;

        // 건데이터 타입이 있으면
        if (!string.IsNullOrEmpty(gunData.Type))
        {
            // 열거형 정의에 타입과 같은 타입을 꺼내서 저장
            System.Enum.TryParse(gunData.Type, out bulletDamageType);
        }

        if (bulletRigidbody == null) // 리지드바디 없으면
        {
            //리지드바디 가져오기
            bulletRigidbody = GetComponent<Rigidbody2D>();
        }

        if (bulletRigidbody != null)  // 리지드바디 있으면
        {
            // 총알 속도 계산
            bulletRigidbody.linearVelocity = new Vector2(dirX * speed, 0f);
        }

        if (bulletSpriteRenderer != null) // 스프라인트 있으면
        {
            // 자식에서 스프라이트 랜더러 가져와서 저장
            bulletSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (dirX < 0) // 왼쪽으로 날아간다면
        {
            bulletSpriteRenderer.flipX = true; // 이미지 뒤집기 O
        }
        else // 오른쪽으로 날아간다면
        {
            bulletSpriteRenderer.flipX = false; // 이미지 뒤집기 X
        }
    }

    private void Update()
    {
        // 총알 현재 위치와 출발위치 저장
        float traveledDistance = Vector2.Distance(_startPosition, transform.position);

        // 위치가 사거리보다 크면
        if (traveledDistance >= _effectiveRange)
        {
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

            // 만약 적 스텟이 있고, 배틀매니저가 있으면
            if (enemyStat != null && BattleManager.Instance != null)
            {
                int calculatedDamage = damage; // 기본 대미지 저장

                // 총알 타입이 샷건이라면
                if (bulletDamageType == DamageType.Shotgun)
                {
                    // 스타트 포인트와, 현재 거리 저장
                    float hitDistance = Vector2.Distance(_startPosition, transform.position);

                    // 날아간 거리가 최대 사거리의 절반 이하라면
                    if (hitDistance <= (_effectiveRange / 2f))
                    {
                        calculatedDamage = damage * 3; // 대미지 3배
                        UtillLogRemove.Log("샷건 근접 사격 3배 대미지 : " + calculatedDamage);
                    }
                    else // 절반 이상이면
                    {
                        // 기본 대미지
                        calculatedDamage = damage;
                        UtillLogRemove.Log("샷건 원거리 사격 기본 대미지 : " + calculatedDamage);
                    }
                }

                // 배틀매니저에서 적 정보, 줄 데미지, 총알 타입을 넘겨주고 플레이어 공격 함수 호출
                bool isCorrectTarget = BattleManager.Instance.ProcessPlayerAttack(enemyStat, calculatedDamage, bulletDamageType);
               
                // 타겟이 트루 면
               if (isCorrectTarget == true)
               {
                    // 총알 데미지 타입이 저격이면
                    if (bulletDamageType == DamageType.Sniper)
                    {
                        // 적 현재 타입이 노멀이면
                        if (enemyStat.CurrentType == ZombieType.Normal)
                        {
                            // 관톧 카운터 +1
                            _pierceCount = _pierceCount + 1;
                            UtillLogRemove.Log("저격 관통 수" + +_pierceCount + " / 3");

                            if (_pierceCount >= 3) // 관통수가 3 이상이면
                            {
                                UtillLogRemove.Log("저격탄 최대 관통 수 도달 소멸");
                                gameObject.DeactivateSafe(); // 총알 비활성 화
                            }
                        }
                        // 현재 적타입이 특수라면
                        else if (enemyStat.CurrentType == ZombieType.Special)
                        {
                            UtillLogRemove.Log("저격탄이 특수 좀비 공격");
                            gameObject.DeactivateSafe(); // 총알 비활성 화
                        }
                    }
                    else // 저격총이 아니면 관통 없음
                    {
                        // 총알 비활성화
                        gameObject.DeactivateSafe();
                    }
               }
            }
        }
        // 만약 땅 타일맵(Default 레이어 등)에 부딪히면
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            gameObject.DeactivateSafe(); // 총알 비활성화
        }
    }
}
