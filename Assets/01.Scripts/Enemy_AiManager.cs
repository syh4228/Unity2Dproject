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
    
    private SpriteRenderer spriteRenderer;

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
                return; // 반환
            }
        }

        if (isAttack == true)
        {
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
            AttackPlayer(); // 공격 함수 호출
        }
        else if (distanceToPlayer <= detectRange) // 만약 추적 사거리 안에 있으면
        {
            ChasePlayer(); // 추적 함수 호출
        }
        else // 둘다 아니면
        {
            animController.SetState(AllState.Idle); // 대기
        }
    }

    private void ChasePlayer() // 플레이어 추적 함수
    {
        animController.SetState(AllState.Run); // 달리기 애니매이션 실행

        // 만약 플레이어가 나보다 왼쪽에 있으면
        if (playerTransform.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true; //  스프라이트 그대로 
        }
        else
        {
            spriteRenderer.flipX = false; // 반대면 스프라이트 반전
        }

        // 플레이어 추적
        Vector2 targetPosition = new Vector2(playerTransform.position.x, transform.position.y);

        // 이동 속도 계산
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    private void AttackPlayer() // 공격 함수
    {
        animController.SetState(AllState.Attack); // 공격 애니메이션 실행
        isAttack = true; // 공격 쿨타임 시작

    }

}
