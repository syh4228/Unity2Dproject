using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Enemy_StatManager : MonoBehaviour
{
    [Header("좀비 타입")]
    public ZombieType CurrentType; // 좀비 타입 변수 저장
    public string enemyId; // 적 Id 저장
    public string enemyName; // 적 이름 저장

    [Header("체력 설정")]
    public int MaxHp = 100;// 최대 체력
    public int currentHp; // 현재 체력
    public int Attack = 5; // 공격력

    [Header("컴포넌트 연결")]
    [SerializeField] private Enemy_AiManager enemyAI; // 적 Ai 와 연결
    [SerializeField] private Enemy_AnimationController animController; // 애니메이션 컨트롤러 연결

    private bool _isDead = false; // 죽음 체크

    public event Action<int, int> OnHpChanged; // UI에 최대체력과, 현재 체력 알려주기



    private void Start()
    {
        currentHp = MaxHp; // 시작시 최대체력은 = 현재체력

        if (enemyAI == null)
        {
            enemyAI = GetComponent<Enemy_AiManager>();
        }

        if (animController == null)
        {
            animController = GetComponent<Enemy_AnimationController>();
        }
    }

    // JSON 데이터 받아오는 함수
    public void Initialize(DNMonsterData monsterData)
    {
        enemyId = monsterData.Id; // Id 저장
        enemyName = monsterData.Name; // 이름 저장

        MaxHp = monsterData.BaseHp; // 체력 저장
        currentHp = MaxHp; // 현재체력 최대체력으로 저장
        Attack = monsterData.BaseAtk; // 공격력 저장

        // 타입이 노멀이면 노멀로 저장, 아니면 스폐셜로 타입 저장
        CurrentType = (monsterData.Type == "Normal") ? ZombieType.Normal : ZombieType.Special;

        // 확인로그
        UtillLogRemove.Log(enemyName + " 데이터 다운완료! 체력: " + MaxHp + ", 공격력: " + Attack);
    }

    public void TakeDamage(int damage) // 외부에서 대미지 받아오는 함수
    {
        if (_isDead == true) // 만약 죽었으면 
        {
            return; // 반환
        }

        currentHp -= damage; // 대미지 만큼 체력 감소
        UtillLogRemove.Log(enemyName + " 피격, 남은 체력: " + currentHp);

        if (currentHp <= 0) // 만약 체력이 0이하면
        {
            currentHp = 0; // 현재 체력 0
            Die(); // 사망함수 호출
        }
        else // 죽지 않았다면
        {
            if(enemyAI != null) // 적 AI 연결 되있으면
            {
                enemyAI.TriggerHitStun(); // ai매니저 피격함수 호출
            }
        }

        if (OnHpChanged != null) // OnHpChanged가 있으면
        {
            OnHpChanged(currentHp, MaxHp);// 현재최력, 최대체력 알리기
        }
    }

    private void Die() // 사망 함수
    {
        _isDead = true; // 죽음 처리

        if (animController != null) // 만약 애니메이션 연결 되있으면
        {
            animController.SetState(AllState.Dead); // 사망 애니메이션 실행
        }

        if (enemyAI != null) // 적 Ai 연결되있으면
        {
            enemyAI.enabled = false; // 연결 해제
        }

        // 사망시 더이상 몬스터와 충돌 방지
        gameObject.layer = LayerMask.NameToLayer("EnemyDead");

        // 비활성화 함수 호출
        DeactivateRoutine(0.5f).Forget();
    }


    // 비활성화 함수
    private async UniTaskVoid DeactivateRoutine(float delay)
    {
        try
        {
            // 지정된 시간만큼 대기 (도중에 적이 삭제되면 자동 취소됨)
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy());

            // 대기가 무사히 끝나면 오브젝트 풀로 반환 (비활성화)
            gameObject.SetActive(false);
        }
        catch (OperationCanceledException)
        {
            // 에러 무시 (씬 전환 등으로 적이 이미 파괴된 경우 안전하게 넘김)
        }
    }

    // 적 리셋 함수
    public void ResetEnemy()
    {
        _isDead = false; // 죽지 안았다면
        currentHp = MaxHp; // 체력 회복

        gameObject.layer = LayerMask.NameToLayer("Enemy"); // 레이어 복구

        if (enemyAI != null)
        {
            enemyAI.enabled = true; // 적 AI 연결
        }

        if (animController != null)
        {
            animController.SetState(AllState.Idle); // 대기 모션으로
        }

    }
}
