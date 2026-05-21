using UnityEngine;
using System.Collections;

public class Enemy_AiManager : MonoBehaviour
{
    [Header("행동 설정")]
    public float moveSpeed = 1f; // 이동 속도
    public float detectRange = 8f; // 감지 범위
    public float attackRange = 1.5f; // 공격 범위
    public float attackCooldown = 2f; // 공격 쿨타임

    [Header("경직 설정")]
    [SerializeField] private float stunTime = 0.5f; // 경직 시간
    public bool isStunned = false; // 피격 확인

    [Header("컴포넌트")]
    [SerializeField] private AnimationController animController; // 애니메이터 컨트롤러 연결
    [SerializeField] private Enemy_StatManager statManager; // 스탯 매니저 연결

    private SpriteRenderer spriteRenderer; // 스프라이트 랜더러 받기
    private Rigidbody2D enemyRigidbody; // 리지드바디 받기

    private Transform playerTransform; // 플레이어 위치 받기
    private bool isAttack = false; // 공격중인지 확인

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
        // 게임매니저가 있고
        if (GameManager.Instance != null)
        {
            // 게임매니저에서 전투중이 아닐때
            if (!GameManager.Instance.IsBattleActive)
            {
                return;
            }
        }

        if (playerTransform == null) // 플레이어를 못찾았으면
        {
            return; // 반환
        }

        if (playerTransform != null) // 플레이어 트랜스폼이 있으면
        {
            // 플레이어 컴포넌트 정보 가져오기
            Player_Controller player = playerTransform.GetComponent<Player_Controller>();

            if (player != null) // 플레이어가 있으면
            {
                if (player.IsDead == true) // 플레이어가 죽었으면
                {
                    // 속도 0 (멈추기)
                    enemyRigidbody.linearVelocity = new Vector2(0, enemyRigidbody.linearVelocity.y);
                    animController.SetState(AllState.Idle); // 대기애니메이션 실행
                    return; // 반환
                }
            }
        }

        if (statManager != null) // 스탯 매니저가 연결되있고
        {
            if (statManager.currentHp <= 0) // 현재 체력이 0이면
            {
                // 죽으면 속도 0
                enemyRigidbody.linearVelocity = new Vector2(0, enemyRigidbody.linearVelocity.y);
                return; // 반환
            }
        }

        if (isStunned == true) // 피격이 트루면
        {
            // 밀리는 힘은 받되, 스스로 걷지는 못하게 속도 0으로 고정
            enemyRigidbody.linearVelocity = new Vector2(0, enemyRigidbody.linearVelocity.y);
            return;
        }

        if (isAttack == true)
        {
            // 공격중 이동 금지
            enemyRigidbody.linearVelocity = Vector2.zero;

            return; // 반환
        }

        // 본인과 플레이어 위치 계산
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= 0.8f) // 만약 플레이어가 0.8f 보다 가까이 있으면
        {
            enemyRigidbody.linearVelocity = Vector2.zero; // 이동속도 0
            animController.SetState(AllState.Idle); // 대기 애니메이션 실행
        }
        else if(distanceToPlayer <= attackRange) // 만약 플레이어가 공격 사거리 안에 있으면
        {
            AttackPlayer(); // 공격 함수 호출
        }
        else if(distanceToPlayer  <= detectRange) // 만약 플레이어가 추적 사거리 안에 있으면
        {
            ChasePlayer(); // 추적 함수 호출
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
        if (isAttack) // 공격 중이면 
        {
            return; // 반환
        }

        isAttack = true; //  공격 상태 변환
        // 공격 애니메이션 실행
        animController.SetState(AllState.Attack);
        // 공격 쿨타임 함수호출
        StartCoroutine(AttackRoutine());
    }

    // 피격 트리거 함수
    public void TriggerHitStun()
    {
        // 스택매니저가 있고
        if (statManager != null)
        {
            // 체력이 0이 아니면
            if (statManager.currentHp > 0) { }
            {
                // 스턱 시간 함수 호출
                StartCoroutine(HitStunRoutine());
            }
        }
    }

    private IEnumerator HitStunRoutine()
    {
        isStunned = true; // 피격 확인
        isAttack = false; // 공격중 맞았다면 공격 취소

        if (animController != null)  // 애니메이이션 컨트롤러 있으면
        {
            // 피격 애니메이션 실행
            animController.AllHitAnimation();
        }

        yield return new WaitForSeconds(stunTime); // 경직

        if (animController != null)
        {
            animController.SetState(AllState.Idle);
        }

        isStunned = false; // 피격 확인 끝
    }

    // 공격 쿨타임 함수
    private IEnumerator AttackRoutine()
    {
        // 0.3초 뒤에 (공격 모션과 맞게 대미지 주기)
        yield return new WaitForSeconds(0.3f);
        // 플레이어 맞는지 확인 함수 호출
        ApplyDamageToPlayer();

        // 0.2초 후에 ( 공격 애니메이션 끝나기 기다리기)
        yield return new WaitForSeconds(0.2f);
        // 대기 애니메이션 재생 호출
        animController.SetState(AllState.Idle);

        // 디버그 쿨타임은 어택 쿨 - 0.5f (0.5초보다 공격 쿨이 작을경우 방지)
        float DebugCooldown = attackCooldown - 0.5f;

        if (DebugCooldown > 0) // 만약 디버그 쿨타임이 0보다 크면
        {
            // 디버그 쿨타임 시작
            yield return new WaitForSeconds(DebugCooldown);
        }

        // 공격 가능
        isAttack = false;
    }

    // 공격 범위에 있는게 플레이어인지 확인하는 함수
    public void ApplyDamageToPlayer()
    {
        // 공격범위 안에 있는 물체 정보 가져오기
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRange);

        // 찾은 물체 하나씩 확인
        foreach (Collider2D col in hitPlayers)
        {
            // 태그가 플레이어면
            if (col.CompareTag("Player"))
            {
                // 컴포넌트 가져오기
                Player_Character playerStat = col.GetComponent<Player_Character>();

                // 컴포넌트 가져왔으면
                if (playerStat != null)
                {
                    if (statManager != null) // 스탯매니저가 있으면
                    {
                        // 배틀매니저에 플레이어 대미지 주는 함수 호출
                        BattleManager.Instance.ProcessEnemyAttack(playerStat, statManager.Attack);

                        break;
                    }
                }
            }
        }
    }
}
