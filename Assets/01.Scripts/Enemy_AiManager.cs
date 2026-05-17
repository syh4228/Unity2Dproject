using UnityEngine;

public class Enemy_AiManager : MonoBehaviour
{
    [Header("행동 설정")]
    public float moveSpeed = 1f; // 이동 속도
    public float detectRange = 8f; // 감지 범위
    public float attackRange = 1.5f; // 공격 범위
    public float attackCooldown = 2f; // 공격 쿨타임

    [Header("컴포넌트")]
    [SerializeField] private AnimationController animController; // 애니메이터 컨트롤러 연결
    [SerializeField] private Enemy_StatManager statManager; // 스탯 매니저 연결
    
    private SpriteRenderer spriteRenderer; // 스프라이트 랜더러 받기
    private Rigidbody2D enemyRigidbody; // 리지드바디 받기

    private Transform playerTransform; // 플레이어 위치 받기
    private bool isAttack = false; // 공격중인지 확인
    private float attackTime = 0f; // 공격 쿨타임 재기

    private void Start()
    {
        if (animController == null) // 애니메이션 컴포넌트가 연결안되있으면
        {
            animController = GetComponent<AnimationController>(); // 찾기
        }

        if (statManager == null) // 스탯매니저 연결 안되있으면
        {
            statManager = GetComponent<Enemy_StatManager>(); // 찾기
        }

        // 자식 오브젝트에서 스프라이트 랜더러 가져오기
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        enemyRigidbody = GetComponent<Rigidbody2D>(); // 리지디바디 가져오기

        // 플레이어 태그 가진 오브젝트 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null) // 플레이어 오브젝트 있으면
        {
            playerTransform = playerObj.transform; // 플레이어 위치 찾기
        }
        else
        {
            // 만약 플레이어 태그를 못 찾으면 에러 띄우기
            UtillLogRemove.Error("플레이어를 찾을 수 없습니다! 플레이어 오브젝트의 Tag가 'Player'인지 확인해주세요.");
        }
    }

    private void Update()
    {
        if (playerTransform == null) // 플레이어를 못찾았으면
        {
            return; // 반환
        }

        if (statManager  != null) // 스탯 매니저가 연결되있고
        {
            if (statManager.currentHp <= 0) // 현재 체력이 0이면
            {
                // 죽으면 속도 0
                enemyRigidbody.linearVelocity = new Vector2(0, enemyRigidbody.linearVelocity.y);
                return; // 반환
            }
        }

        if (isAttack == true)
        {
            // 공격중에는 속도 0
            enemyRigidbody.linearVelocity = new Vector2(0, enemyRigidbody.linearVelocity.y);

            attackTime += Time.deltaTime; // 쿨타임 시작

            if (attackTime >= 0.1f) // 공격 시작하고, 0.5초 후
            {
                animController.SetState(AllState.Idle); // 대기 애니메이션으로 전환
            }

            if (attackTime >= attackCooldown)  // 쿨타임 시간이 끝나면
            {
                isAttack = false; // 공격 가능
                attackTime = 0f; // 쿨 초기화
            }

            return; // 반환
        }

        // 본인과 플레이어 위치 계산
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange) // 만약 공격 사거리 안에 있으면
        {
            // 공격 시전시 속도 0
            enemyRigidbody.linearVelocity = new Vector2(0, enemyRigidbody.linearVelocity.y);
            AttackPlayer(); // 공격 함수 호출
        }
        else if (distanceToPlayer <= detectRange) // 만약 추적 사거리 안에 있으면
        {
            ChasePlayer(); // 추적 함수 호출
        }
        else // 둘다 아니면
        {
            // 속도 0 
            enemyRigidbody.linearVelocity = new Vector2(0, enemyRigidbody.linearVelocity.y);
            animController.SetState(AllState.Idle); // 대기
        }
    }

    private void ChasePlayer() // 플레이어 추적 함수
    {
        animController.SetState(AllState.Run); // 달리기 애니매이션 실행

        float directionX = 1f; // 이동 방향을 저장 변수

        // 만약 플레이어가 나보다 왼쪽에 있으면
        if (playerTransform.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true; //  스프라이트 그대로 
            directionX = -1f; // 왼쪽 보기
        }
        else
        {
            spriteRenderer.flipX = false; // 반대면 스프라이트 반전
            directionX = 1f; // 오른쪽 보기
        }

        enemyRigidbody.linearVelocity = new Vector2(directionX * moveSpeed, enemyRigidbody.linearVelocity.y);
    }

    private void AttackPlayer() // 공격 함수
    {
        animController.SetState(AllState.Attack); // 공격 애니메이션 실행
        isAttack = true; // 공격 쿨타임 시작

        // 공격범위 가져오기
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRange);

        // 콜라이더2D가 공격범위에 들어오면
        foreach (Collider2D col in hitPlayers)
        {
            if (col.CompareTag("Player")) // 만약 플레이어라면
            {
                // 플래이어 컴포넌트 가져오기
                Player_Character playerStat = col.GetComponent<Player_Character>();
                // 플레이어 컴포넌트가 있고, 스탯매니저가 있으면
                if (playerStat != null && statManager != null)
                {
                    // 만약 배틀매니저가 있으면
                    if (BattleManager.Instance != null)
                    {
                        // 배틀매니저에서 함수 호출
                        BattleManager.Instance.ProcessEnemyAttack(playerStat, statManager.Attack);
                    }
                }
                break;
            }
        }

    }

}
