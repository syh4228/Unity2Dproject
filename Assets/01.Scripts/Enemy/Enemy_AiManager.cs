using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Enemy_AiManager : MonoBehaviour
{
    [Header("행동 설정")]
    public float moveSpeed = 1f; // 이동 속도
    public float detectRange = 8f; // 감지 범위
    public float attackRange = 1.5f; // 공격 범위
    public float attackCooldown = 2f; // 공격 쿨타임
    private float lastAttackTime = 0f; // 공격 쿨타임 계산용

    [Header("경직 설정")]
    [SerializeField] private float stunTime = 0.5f; // 경직 시간
    public bool isStunned = false; // 피격 확인

    [Header("컴포넌트")]
    [SerializeField] private Enemy_AnimationController animController; // 애니메이터 컨트롤러 연결
    [SerializeField] private Enemy_StatManager statManager; // 스탯 매니저 연결


    private SpriteRenderer spriteRenderer; // 스프라이트 랜더러 받기
    private Rigidbody2D enemyRigidbody; // 리지드바디 받기
    private Transform playerTransform; // 플레이어 위치 받기
    private Player_Controller playerController; // 플레이어 컨트롤 받기

    private bool isAttack = false; // 공격중인지 확인
    public bool isKnockedBack = false; // 밀치기 확인 변수
    public Transform decoyTarget; // 슈륙탄 어그로 위치 저장

    private EnemySkill_Beast beastSkill; // 비스트 연결
    private EnemySkill_Bomber bomberSkill; // 바머 연결
    private EnemySkill_Thrower throwerSkill; // 쓰로머 연결
    private EnemySkill_auger augerSkill; // 오거 연결

    private void Start()
    {
        if (animController == null) // 애니메이션 컴포넌트가 연결안되있으면
        {
            animController = GetComponent<Enemy_AnimationController>(); // 찾기
        }

        if (statManager == null) // 스탯매니저 연결 안되있으면
        {
            statManager = GetComponent<Enemy_StatManager>(); // 찾기
        }

        beastSkill = GetComponent<EnemySkill_Beast>(); // 비스트 컴포넌트 가져오기
        bomberSkill = GetComponent<EnemySkill_Bomber>(); // 바머 가져오기
        throwerSkill = GetComponent<EnemySkill_Thrower>(); // 쓰로머 가져오기
        augerSkill = GetComponent<EnemySkill_auger>(); // 오거 가져오기

        // 자식 오브젝트에서 스프라이트 랜더러 가져오기
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        enemyRigidbody = GetComponent<Rigidbody2D>(); // 리지디바디 가져오기

        // 플레이어 태그 가진 오브젝트 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null) // 플레이어 오브젝트 있으면
        {
            playerTransform = playerObj.transform; // 플레이어 위치 찾기
            // 플레이어 컨트롤 컴포너트 가져오기
            playerController = playerObj.GetComponent<Player_Controller>();
        }
        else
        {
            // 만약 플레이어 태그를 못 찾으면 에러 띄우기
            UtillLogRemove.Error("플레이어를 찾을 수 없습니다! 플레이어 오브젝트의 Tag가 'Player'인지 확인해주세요.");
        }
    }

    private void Update()
    {
        // 게임매니저가 인스턴스가 있고, 게임매니저의 배틀액션이 거짓이면 반환
        if (GameManager.Instance != null && GameManager.Instance.IsBattleActive == false) return;

        // 플레이어가 없거나, 넉백중이거나, 스턴 중이거나,공격중이면
        if (playerTransform == null || isKnockedBack == true || isStunned == true || isAttack == true)
        {
            // 공격중이거나, 스턴 중이면
            if (isAttack == true || isStunned == true)
            {
                // 속동 0으로 고정(미끄러지지 않게)
                enemyRigidbody.linearVelocity = new Vector2(0, enemyRigidbody.linearVelocity.y);
            }
            return; // 반환
        }

        // 스탯매니저가 있고, 스탯매니저에 현재체력이 0 보다 작으면 반환
        if (statManager != null && statManager.currentHp <= 0) return;

        // 플레이어 컨트롤이 있고, 플레이어가 죽었다면
        if (playerController != null && playerController.IsDead == true)
        {
            // 속도 0으로 고정
            enemyRigidbody.linearVelocity = new Vector2(0, enemyRigidbody.linearVelocity.y);
            // 애니메이션 대기로 변경
            animController.SetState(AllState.Idle);
            return; // 반환
        }

        // 디코이 타겟이 있으면, 디코이 타겟 저장, 아니면 플레이어 저장
        Transform activeTarget = (decoyTarget != null) ? decoyTarget : playerTransform;
        // 타겟과의 거리 계산
        float distanceToTarget = Vector2.Distance(transform.position, activeTarget.position);

        // 비스트 스킬이 있고, 비스트 점프 공격 레이지 안에 타겟이 있고, 공격범위 안에 타겟이 없으면
        if (beastSkill != null && distanceToTarget <= beastSkill.jumpAttackRange && distanceToTarget > attackRange)
        {
            // 비스트 공격 준비 함수 호출
            ExecuteSpecialBeastAttack();
            return;
        }

        if (distanceToTarget <= attackRange) // 타켓이 공격 범위 안에 있으면
        {
            // 공격범위에 들어오면 속도 0 고정
            enemyRigidbody.linearVelocity = Vector2.zero;

            if (bomberSkill != null) // 바머 스킬이 있으면
            {
                // 바머 자폭 함수 호출
                bomberSkill.ExecuteExplosion(playerTransform, statManager);
            }
            else if (throwerSkill != null) // 쓰로머 스킬 있으면
            {
                // 쓰로머 공격 준비 함수 호출
                ExecuteSpecialThrowAttack();
            }
            else if (augerSkill != null)
            {
                // 오거 공격 준비 함수 호출
                ExecuteSpecialAugerAttack();
            }
            else
            {
                // 플레이어 공격 함수 호출
                AttackPlayer();
            }
        }

        // 거리가 감지범위보다 작거나, 타겟이 디코이 타겟이면
        else if (distanceToTarget <= detectRange || activeTarget == decoyTarget)
        {
            ChaseTarget(activeTarget); // 타겟 디코이로 변환
        }
    }

    private void ChaseTarget(Transform target) // 플레이어 추적 함수
    {
        animController.SetState(AllState.Run); // 달리기 애니매이션 실행

        float directionX = 1f; // 이동 방향을 저장 변수

        // 만약 플레이어가 나보다 왼쪽에 있으면
        if (target.position.x < transform.position.x)
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
        if (isAttack == true) // 공격 중이면 
        {
            return; // 반환
        }

        // 마지막 공격 시간과 공격 쿨타임 더한 시간이 현재시간보다 많으면
        if (Time.time < lastAttackTime + attackCooldown)
        {
            // 애니메이션 대기로 변경
            animController.SetState(AllState.Idle);
            return; // 반환
        }

        isAttack = true; //  공격 상태 변환
        lastAttackTime = Time.time;

        // 공격 애니메이션 실행
        animController.SetState(AllState.Attack);
        // 공격 쿨타임 함수호출
        AttackRoutine().Forget();
    }

    // 비스트 공격 준비 함수
    private async void ExecuteSpecialBeastAttack()
    {
        isAttack = true; // 공격이 트루 변경
        enemyRigidbody.linearVelocity = Vector2.zero; // 속도 0 고정
        // 비스트 공격함수 호출
        await beastSkill.ExecuteBeastAttack(playerTransform, animController);
        isAttack = false; // 공격 거짓으로 변경
    }

    // 쓰로머 공격 준비 함수
    private async void ExecuteSpecialThrowAttack()
    {
        isAttack = true;
        enemyRigidbody.linearVelocity = Vector2.zero;
        await throwerSkill.ExecuteThrowAttack(playerTransform, animController, attackCooldown);
        isAttack = false;
    }

    // 오거 공격 준비 함수
    private async void ExecuteSpecialAugerAttack()
    {
        isAttack = true;
        enemyRigidbody.linearVelocity = Vector2.zero;
        await augerSkill.ExecuteAugerAttack(playerTransform, animController, attackCooldown);
        isAttack = false;
    }

    // 피격 트리거 함수
    public void TriggerHitStun()
    {
        // 스택매니저가 있고 체력이 0이 아니면
        if (statManager != null && statManager.currentHp > 0)
        {
            if (augerSkill != null) // 오거 스킬이 있으면
            {
                augerSkill.ApplySlowdown(); // 오거 슬로우  함수 호출
            }
            else // 오거 아니면
            {
                // 스턱 시간 함수 호출
                HitStunRoutine().Forget();
            }
        }
    }

    // 피격 유니테스크 함수
    private async UniTaskVoid HitStunRoutine()
    {
        isStunned = true; // 피격 확인
        isAttack = false; // 공격중 맞았다면 공격 취소

        if (animController != null)  // 애니메이이션 컨트롤러 있으면
        {
            // 피격 애니메이션 실행
            animController.SetState(AllState.Hit);
        }

        try
        {
            // 피격 스턴 시간 만큼 대기, 스턴도중 죽으면 취소
            await UniTask.Delay(TimeSpan.FromSeconds(stunTime), cancellationToken: this.GetCancellationTokenOnDestroy());

            if (animController != null) // 애니메이션 컨트롤러 있으면
            {
                // 애니메이션 대기로 변경
                animController.SetState(AllState.Idle);
            }
        }
        catch (OperationCanceledException)
        {
            // 몬스터가 죽어서 파괴되었을 때 발생하는 에러 방지
        }
        finally
        {
            // 스턴 종료
            isStunned = false;
        }
    }

    // 밀쳐기 당한  함수
    public void ApplyShove(Vector2 pushDirection, float shoveForce, float shoveStunTime)
    {
        // 스탯매니저가 있고 체력이 0보다 작으면 반환
        if (statManager != null && statManager.currentHp <= 0) return;

        if (augerSkill != null) // 오거 스킬 있으면
        {
            augerSkill.ApplySlowdown(); // 오거 슬로우 함수 호출
            UtillLogRemove.Log("오거는 밀치기에 면역");
            return;
        }

        // 비스트 스킬이 있고, 비스트가 채공중이면
        if (beastSkill != null && beastSkill.isLeaping == true)
        {
            // 비스트 점프 추락 함수 호출
            beastSkill.InterruptJump(isInstaKill: false);
        }

        // 밀치키 유니테스크 호출
        ShoveRoutine(pushDirection, shoveForce, shoveStunTime).Forget();
    }

    // 밀치기 유니테스크 함수
    private async UniTaskVoid ShoveRoutine(Vector2 pushDirection, float shoveForce, float shoveStunTime)
    {
        isKnockedBack = true; // Update에서 속도 0 고정을 막기 위해 켜줌
        isAttack = false;     // 공격 중이었다면 취소

        if (animController != null)
        {
            animController.SetState(AllState.Hit); // 밀쳐질 때도 피격 애니메이션 재생
        }

        // 속도를 0으로
        enemyRigidbody.linearVelocity = Vector2.zero;

        // 밀쳐진 위치 계산 적용.
        enemyRigidbody.AddForce(pushDirection * shoveForce, ForceMode2D.Impulse);

        try
        {  
            // 밀치기 스턴 시간만큼 대기, 도중에 죽으면 취소
            await UniTask.Delay(TimeSpan.FromSeconds(shoveStunTime), cancellationToken: this.GetCancellationTokenOnDestroy());

            if (animController != null)
            {
                animController.SetState(AllState.Idle);
            }
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            isKnockedBack = false; // 스턴 종료
        }
    }

    // 공격 쿨타임 함수
    private async UniTaskVoid AttackRoutine()
    {
        try
        {
            // 공격 애니미에션에 맞춰 대기시간 0.3초, 도중에 죽으면 취소
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: this.GetCancellationTokenOnDestroy());
            ApplyDamageToPlayer(); // 플레이어게 대미지 알려주기 함수 호출

            // 공격 애니메이션에 맞춰 대기시간 0.2초, 도중에 죽으면 취소
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: this.GetCancellationTokenOnDestroy());
            animController.SetState(AllState.Idle); // 애니메이션 대기로 변경

        }
        catch (OperationCanceledException)
        {
            // 무시
        }
        finally
        {
            isAttack = false; // 공격중 종료
        }
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
            if (col.CompareTag("Player") == true)
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

    // 디코이 함수
    public void SetDecoy(Transform decoy)
    {
        // 디코이 타겟팅 저장
        decoyTarget = decoy;
    }
}
