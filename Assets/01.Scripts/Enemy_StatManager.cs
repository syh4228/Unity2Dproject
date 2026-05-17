using System;
using UnityEngine;

public class Enemy_StatManager : MonoBehaviour
{
    [Header("체력 설정")]
    public int MaxHp = 100;// 최대 체력
    public int currentHp; // 현재 체력
    public int Attack = 5; // 공격력

    [Header("컴포넌트 연결")]
    [SerializeField] private Player_Controller enemyController; // 실제 사망처리할 플레이어 컨트롤러 연결

    private bool _isDead = false; // 죽음 체크

    public event Action<int, int> OnHpChanged; // UI에 최대체력과, 현재 체력 알려주기

    private void Start()
    {
        currentHp = MaxHp; // 시작시 최대체력은 = 현재체력

        if (enemyController == null)
        {
            enemyController = GetComponent<Enemy_AiManager>();
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

        if (OnHpChanged != null) // OnHpChanged가 있으면
        {
            OnHpChanged(currentHp, MaxHp);// 현재최력, 최대체력 알리기
        }
    }

    private void Die() // 사망 함수
    {
        _isDead = true; // 죽음 처리

        if (enemyController != null) // 만약 플레이어 컨트롤러가 연결 되있으면
        {
            enemyController.Die(); // 플레이어 컨트롤러 죽음 함수 호출
        }

        // 사망시 더이상 몬스터와 충돌 방지
        gameObject.layer = LayerMask.NameToLayer("EnemyDead");
    }
}
