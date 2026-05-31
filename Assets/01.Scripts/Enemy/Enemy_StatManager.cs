using System;
using UnityEngine;
using System.Collections;

public class Enemy_StatManager : MonoBehaviour
{
    [Header("좀비 타입")]
    public ZombieType CurrentType; // 좀비 타입 변수 저장

    [Header("체력 설정")]
    public int MaxHp = 100;// 최대 체력
    public int currentHp; // 현재 체력
    public int Attack = 5; // 공격력

    [Header("컴포넌트 연결")]
    [SerializeField] private Enemy_AiManager enemyAI; // 적 Ai 와 연결
    [SerializeField] private AnimationController animController; // 애니메이션 컨트롤러 연결

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
            animController = GetComponent<AnimationController>();
        }
    }

    public void TakeDamage(int damage) // 외부에서 대미지 받아오는 함수
    {
        if (_isDead == true) // 만약 죽었으면 
        {
            return; // 반환
        }

        currentHp -= damage; // 대미지 만큼 체력 감소
        UtillLogRemove.Log("적 피격, 남은 체력:{currentHp}");

        if (currentHp <= 0) // 만약 체력이 0이하면
        {
            currentHp = 0; // 현재 체력 0
            Die(); // 사망함수 호출
        }
        else // 죽지 않았다면
        {
            // 적 ai매니저에서 컴포넌트 받아오기
            Enemy_AiManager aiManager = GetComponent<Enemy_AiManager>();

            if(aiManager != null) // 컴포넌트 받아 왔으면
            {
                aiManager.TriggerHitStun(); // ai매니저 피격함수 호출
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
        StartCoroutine(DeactivateRoutine(0.5f));
    }


    // 비활성화 함수
    private IEnumerator DeactivateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay); // 대기
        gameObject.SetActive(false); // 오브젝트 풀 반환
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
