using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class GrenadeManager : MonoBehaviour
{
    [Header("슈륙탄 설정")]
    public float throwForce = 8f; // 던지는 힘
    public float explosionRadius = 3f; // 폭발 범위
    public int explosionDamage = 300; // 폭발 대미지
    public float fuseTime = 2f; // 폭발 지연 시간

    [Header("어그로 설정")]
    public float aggroRadius = 10f; // 좀비 어그로 범위

    [Header("이펙트 설정")]
    [SerializeField] private SpriteRenderer grenadeSprite; // 슈류탄 이미지
    [SerializeField] private GameObject explosionEffect; // 폭발 이펙트 오브젝트

    private Rigidbody2D rb; // 리지드바디 저장

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // 리지드 받이 가져오기

        // 폭발 이펙트 이미지가 있으면
        if (explosionEffect != null)
        {
            // 비활성화
            explosionEffect.SetActive(false);
        }
    }

    // 슈류탄 던지기 함수
    public void Toss(float directionX)
    {
        // x축과 y을 좌표 저장
        Vector2 tossDirection = new Vector2(directionX, 0.5f).normalized;
        // 저장된 좌표로 이동
        rb.AddForce(tossDirection * throwForce, ForceMode2D.Impulse);

        // 폭발 타이머 시작
        ExplosionRoutine().Forget();
    }

    // 유니테스크 폭발 함수
    private async UniTaskVoid ExplosionRoutine()
    {
        // 취소 토큰 저장
        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            float elapsedTime = 0f; // 폭발 타이머

            while (elapsedTime < fuseTime) // 타이머가 터지는 시간 보다 작으면
            {
                PullAggroToMe(); // 어그로 함수 호출
                // 대기 시간 0.2초
                await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: token);
                elapsedTime += 0.2f; // 0.2초 씩 추가
            }

            UtillLogRemove.Log("수류탄 펑!");

            // 현재위치에서 폭발 범위안에 적 레이어 가지고 있으면 리스트로 저장
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, LayerMask.GetMask("Enemy"));

            // 하나씩 꺼내서 저장
            foreach (Collider2D hit in hitEnemies)
            {
                // 적 스탯 매니저에서 컴포넌트 받아 저장
                Enemy_StatManager enemyStat = hit.GetComponent<Enemy_StatManager>();

                if (enemyStat != null) // 적 스탯 있으면
                {
                    // 적에게 대미지 전달
                    enemyStat.TakeDamage(explosionDamage);
                }
            }

            if (grenadeSprite != null) // 폭탄 이미지 있으면
            {
                grenadeSprite.enabled = false; // 수류탄 이미지 숨김
            }

            if (explosionEffect != null) // 폭발 이펙트 있으면
            {
                explosionEffect.SetActive(true); // 준비한 폭발 이펙트 켜기
            }

            // 속도 0 으로 변경
            rb.linearVelocity = Vector2.zero;
            // 위치에 고정
            rb.bodyType = RigidbodyType2D.Kinematic;

            // 이펙트를 보여줄 시간 0.5초 기다린 후 완전히 파괴
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
  
            Destroy(gameObject); // 오브젝트 파괴
        }
        catch (OperationCanceledException)
        {
            // 실패시 에러 방지
        }
    }

    // 폭탄 어그로 함수
    private void PullAggroToMe()
    {
        // 현재위치에서 어그로 범위안에 적 레이어 가지고 있으면 리스트로 저장
        Collider2D[] aggroEnemies = Physics2D.OverlapCircleAll(transform.position, aggroRadius, LayerMask.GetMask("Enemy"));
        
        // 하나씩 꺼내서 저장
        foreach (Collider2D hit in aggroEnemies)
        {
            // 적 스탯 매니저에서 컴포넌트 받아 저장
            Enemy_StatManager enemyStat = hit.GetComponent<Enemy_StatManager>();

            // 적 Ai매니저에서 컴포넌트 받아서 저장
            Enemy_AiManager enemyAi = hit.GetComponent<Enemy_AiManager>();

            // 적 스탯 과 적 AI가 있으면
            if (enemyStat != null && enemyAi != null)
            {
                // 적 타입이 노멀이면
                if (enemyStat.CurrentType == ZombieType.Normal)
                {
                    // 적 Ai 디코이 활성
                    enemyAi.SetDecoy(this.transform);
                }
            }
        }
    }

    // 기즈모로 범위 그리기 함수
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; // 색깔 빨강
        // 폭발 범위 기즈모 그리기
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        
        Gizmos.color = Color.green; // 색깔 그린
        // 어그로 범위 기즈모 그리기
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
    }
}
