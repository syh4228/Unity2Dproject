using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class EnemySkill_auger : MonoBehaviour
{
    [Header("패시브 세팅")]
    public float slowMultiplier = 0.5f; // 슬로우 계수
    public float slowDuration = 1f; // 슬로우 지속시간

    [Header("넉백 공격 세팅")]
    public float knockbackForce = 5f;     // 넉백 힘
    public float knockbackStunTime = 0.5f; // 경직 시간

    private float originalSpeed; // 원래 이동속도 저장
    private bool isSlowed = false; // 슬로우 중인지 저장
    private float slowTimer = 0f; // 슬로우 시간 저장

    private Enemy_AiManager ai; // 적Ai 컴포넌트
    private Enemy_StatManager statManager; // 적 스탯 컴포넌트

    private void Start()
    {
        // ai 컴포넌트 가져와서 저장
        ai = GetComponent<Enemy_AiManager>();
        // 스탯 매니저에 컴포넌트 가져와서 저장
        statManager = GetComponent<Enemy_StatManager>();

        // ai 컴포넌트가 있으면 기존 속도 저장
        if (ai != null) originalSpeed = ai.moveSpeed;
    }

    // 슬로우 함수
    public void ApplySlowdown()
    {
        if (ai == null) return; // ai 컴포넌트 없으면 반환

        if (isSlowed == false) // 슬로우 중이 아니면
        {
            // 기존 속도에 슬로우 계수를 곱한 값을 저장
            ai.moveSpeed = originalSpeed * slowMultiplier;
            // 슬로우 중으로 변경
            isSlowed = true;
        }
        // 슬로우 지속 시간 저장
        slowTimer = slowDuration;
    }

    private void Update()
    {
        if (isSlowed == true) // 슬로우 중일때
        {
            // 슬로우 지속시간 감소
            slowTimer -= Time.deltaTime;

            // 슬로우 지속 시간이 0 이하면
            if (slowTimer <= 0f)
            {
                // 기존 속도로 저장
                ai.moveSpeed = originalSpeed;
                // 슬로우중 거짓으로 변경
                isSlowed = false;
            }
        }
    }

    //  오거 넉백 공격 함수
    public async UniTask ExecuteAugerAttack(Transform player, Enemy_AnimationController anim, float cooldown)
    {
        // 애니메이션이 있으면, 공격 애니메이션으로 변경
        if (anim != null) anim.SetState(AllState.Attack);

        try
        {
            // 0.4초 대기, 실패시 취소
            await UniTask.Delay(TimeSpan.FromSeconds(0.4f), cancellationToken: this.GetCancellationTokenOnDestroy());

            // 플레이어 위치가, 공격 범위 + 0.5 안에 있으면
            if (Vector2.Distance(transform.position, player.position) <= ai.attackRange + 0.5f)
            {
                // 플레이어 캐릭터 컴포넌트 가져와 저장
                Player_Character playerStat = player.GetComponent<Player_Character>();
                // 플레이어 컨트롤러 컴포넌트 가져와 저장
                Player_Controller playerController = player.GetComponent<Player_Controller>();

                if (playerStat != null) // 플레이어 컴포넌트 있으면
                {
                    // 배틀매니저에 적 공격 함수 호출
                    BattleManager.Instance.ProcessEnemyAttack(playerStat, statManager.Attack);

                    // 플레이어 컨트롤 컴포넌트 있으면
                    if (playerController != null)
                    {
                        // 플레이어 넉백 계산
                        Vector2 pushDir = (player.position - transform.position).normalized;
                        // 플레이어 컨트롤러에 넉백 함수 호출
                        playerController.ApplyPlayerKnockback(pushDir, knockbackForce, knockbackStunTime);
                    }
                    UtillLogRemove.Log("플레이어 넉백");
                }
            }

            // 0.3초 대기, 실패시 취소
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: this.GetCancellationTokenOnDestroy());
            
            // 애니메이션이 있으면, 대기로 변경
            if (anim != null) anim.SetState(AllState.Idle);

            float debugCooldown = cooldown - 0.7f; // 쿨타임에 - 0.7초 빼서 저장

            // 디버그 쿨타임이 0보다 크면
            if (debugCooldown > 0)
            {
                // 대기
                await UniTask.Delay(TimeSpan.FromSeconds(debugCooldown), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }
        catch (OperationCanceledException) { }
    }
}
