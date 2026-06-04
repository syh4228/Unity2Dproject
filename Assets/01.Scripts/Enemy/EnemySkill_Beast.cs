using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class EnemySkill_Beast : MonoBehaviour
{
    [Header("비스트 공격 세팅")]
    public float jumpAttackRange = 4f;   // 점프 시작 사거리
    public float jumpHeight = 2.5f;      // 포물선 점프 높이
    public float jumpDuration = 0.5f;    // 날아가는 채공 시간

    [Header("마운트(덮치기) 세팅")]
    public int qteTargetMashes = 10;     // 탈출에 필요한 좌우 연타 횟수

    [Header("탈출 시 넉백 세팅")]
    public float breakShoveForce = 8f;   // 밀쳐지는 힘
    public float breakStunTime = 1f;     // 밀쳐진 후 기절해 있는 시간

    public bool isLeaping = false;       // 현재 공중에서 날아가는 중인지
    private bool isMounting = false;     // 현재 플레이어를 덮쳤는지

    private Rigidbody2D rb; // 리지드 바디 저장
    private Enemy_StatManager statManager; // 스탯 매니저 저장

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 리지드바디 가져와 저장

        // 스탯매니저에서 컴포넌트 받아와서 저장
        statManager = GetComponent<Enemy_StatManager>();
    }

    // 점프 공격 함수
    public async UniTask ExecuteBeastAttack(Transform player, Enemy_AnimationController anim)
    {
        UtillLogRemove.Log("비스트 점프 공격!");
        isLeaping = true; // 점프중 트루로 저장

        // 애니메이션 점프공격 변경
        anim.SetState(AllState.JumpAttack);

        float originalGravity = rb.gravityScale; // 중력값 저장
        rb.gravityScale = 0f; // 중력값 0으로 변경
        rb.linearVelocity = Vector2.zero; // 속도 0으로 변경

        Vector2 startPos = transform.position; // 현재위치 저장
        Vector2 targetPos = player.position; // 플레이어 위치 저장
        float elapsed = 0f; // 점프 공격 경과시간 저장

        try
        {
            // 경과시간이 점프중이고, 날아가는 채공 시간 보다 클때
            while (elapsed < jumpDuration && isLeaping == true)
            {
                elapsed += Time.deltaTime; // 경과시간 증가
                float t = elapsed / jumpDuration; // 경과시간을 채공시간으로 나눠서 저장

                // 애니메이션 공격으로 변경
                anim.SetState(AllState.Attack);

                // 현재위치와, 플레이어 위치 직선 경로, 시간 저장
                Vector2 basePos = Vector2.Lerp(startPos, targetPos, t);
                // 포물선 계산 저장
                float arcHeight = Mathf.Sin(t * Mathf.PI) * jumpHeight;

                // 경로되로 포물선으로 이동
                transform.position = basePos + new Vector2(0, arcHeight);

                // 유니테스크 대기, 오브젝트 파괴시 자동 취소
                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }

            // 중력값 원래값으로 저장
            rb.gravityScale = originalGravity;
            
            // 점프중이고, 플레이어와의 거리가 1.5이하면
            if (isLeaping == true && Vector2.Distance(transform.position, player.position) <= 1.5f)
            {
                // 마운트공격 함수 호출
                await StartMountAttack(player, anim);
            }
        }
        catch (OperationCanceledException)
        {
            // 도중 실패시 에러 무시용
        }
        finally // 종료시
        {
            isLeaping = false; // 점프중 거짓으로 변경
            rb.gravityScale = originalGravity; // 중력값 원래값으로 저장
        }
    }

    // 점프 격추 함수
    public void InterruptJump(bool isInstaKill)
    {
        // 점프중이 거짓이면 반환
        if (isLeaping == false) return;

        isLeaping = false; // 점프중 거짓으로 변환

        if (isInstaKill == true) // 근접공격 당했다면
        {
            UtillLogRemove.Log("비스트가 공중에서 근접 공격을 맞고 즉사");
            statManager.TakeDamage(99999);
        }
    }

    // 마운트 공격 함수
    private async UniTask StartMountAttack(Transform player, Enemy_AnimationController anim)
    {
        isMounting = true; // 마운트 트루로 변경
        // 플레이어 컴포넌트 가져와 저장
        Player_Character playerCharacter = player.GetComponent<Player_Character>();

        UtillLogRemove.Log("비스트 마운트 성공! [좌우 방향키]를 연타하여 탈출하세요!");

        int mashCount = 0; // 연타 횟수 저장
        bool requireLeft = true; // 좌, 우 교차 연타확인 변수
        float damageTimer = 0f; // 마운트 당한 시간 저장

        while (isMounting == true) // 마운트 중이면
        {
            damageTimer += Time.deltaTime; // 당한시간 증가

            if (damageTimer >= 1f) // 당한시간이 1이상이면
            {
                // 플레이어캐릭이 있고, 마운트 중이면
                if (playerCharacter != null && statManager != null)
                {
                    // 공격력 저장
                    int finalDamage = statManager.Attack;

                    // 배틀매니저에 플레이어 공격 대미지 전달
                    BattleManager.Instance.ProcessEnemyAttack(playerCharacter, finalDamage);

                    UtillLogRemove.Log("비스트가 물어뜯는 중! 데미지: " + finalDamage + " (연타: " + mashCount + "/" + qteTargetMashes + ")");
                }
                damageTimer = 0f; // 당한 시간 0으로 초기화
            }

            // 좌 입력이 타이밍 트루이고, 방향키 좌를 누르거나, A키를 누르면
            if (requireLeft == true && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)))
            {
                mashCount++; // 연타 카운트 증가
                requireLeft = false; // 좌 입력 타이밍 거짓으로 변경
            }
            // 좌 입력 타이밍이 거짓이고, 뱡항키 우를 누르거나, D키를 누르면
            else if (requireLeft == false && (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)))
            {
                mashCount++; // 연타 카운터 증가
                requireLeft = true; // 좌 입력 타이밍 트루로 변경
            }

            if (mashCount >= qteTargetMashes) // 연타 카운트가 필요한 연타 횟수이상이면
            {
                BreakFree(); // 탈출 함수 호출
                break;
            }

            // 유니테스크 대기
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }
    }

    // 탈출 함수 호출
    public void BreakFree() 
    {
        // 마운트 중이 아니면 반환
        if (isMounting == false) return;

        isMounting = false; // 마운트 중 거짓으로 변경
        UtillLogRemove.Log("탈출 성공");

        // 적 Ai 컴포넌트 가져오기
        Enemy_AiManager ai = GetComponent<Enemy_AiManager>();

        if (ai != null) // 적 Ai가 있으면
        {
            // 넉백과 스턴 당하기
            ai.ApplyShove(new Vector2(-Mathf.Sign(transform.localScale.x), 1f).normalized, breakShoveForce, breakStunTime);
        }
    }
}
