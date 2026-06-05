using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class EnemySkill_Thrower : MonoBehaviour
{
    [Header("투척 세팅")]
    public GameObject projectilePrefab; // 투사체 프리팹
    public Transform firePoint; // 파편이 생성될 위치
    public int poolSize = 5; // 미리 만들 개수

    private Enemy_StatManager statManager; // 적 스텟 매니저 저장
    private List<GameObject> projectilePool; // 투사체 리스트로 저장

    private float lastAttackTime = -99f;

    private void Start()
    {
        // 적 스탯 매니저 컴포넌트 가져와서 저장
        statManager = GetComponent<Enemy_StatManager>();

        // 투사체 리스트로 저장
        projectilePool = new List<GameObject>();

        // 미리 만들어둘 개수보다 작으면 하나씩 꺼내서 저장
        for (int i = 0; i < poolSize; i++)
        {
            // 프리팹이 있으면
            if (projectilePrefab != null)
            {
                // 프리팹 생성
                GameObject obj = Instantiate(projectilePrefab);
                obj.SetActive(false); // 비활성화
                projectilePool.Add(obj); // 리스트에 보관
            }
        }
    }

    // 유니테스크 투사체 공격 함수
    public async UniTask ExecuteThrowAttack(Transform player, Enemy_AnimationController anim, float cooldown)
    {
        if (Time.time < lastAttackTime + cooldown) return;
        lastAttackTime = Time.time;

        // 애니메이션이 있으면, 공격으로 변환
        if (anim != null) anim.SetState(AllState.Attack);

        try
        {
            // 0.3초대기, 실패시 취소
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: this.GetCancellationTokenOnDestroy());

            if (projectilePrefab != null) // 프리팹이 있으면
            { 
                // 프리팹이 있으면 발사장소, 던질 방향 저장
                Vector2 spawnPos = (firePoint != null) ? firePoint.position : transform.position;

                // 투사체 타겟은 널 (새로 만들지 않는다)
                GameObject targetProjectile = null;

                // 리스트에서 하나씩 꺼내서
                foreach (GameObject p in projectilePool)
                {
                    if (p.activeSelf == false) // 비활성화가 있으면
                    {
                        targetProjectile = p; // 투사체 타겟에 저장
                        break;
                    }
                }

                // 만약 투사체 타겟이 없으면
                if (targetProjectile == null)
                {
                    // 프리팹 가져와서 저장
                    targetProjectile = Instantiate(projectilePrefab);
                    projectilePool.Add(targetProjectile); // 리스트에 추가
                }

                // 투사체 위치는 발사위로 저장
                targetProjectile.transform.position = spawnPos;
                targetProjectile.SetActive(true); // 투사체 활성화

                // 적 투사체 컴포넌트에서 컴포넌트 가져와서 저장
                EnemyProjectile projScript = targetProjectile.GetComponent<EnemyProjectile>();

                // 컴포넌트 있으면
                if (projScript != null)
                {
                    // 던질 방향 계산
                    Vector2 dir = (player.position - (Vector3)spawnPos).normalized;
                    // 투사체 정보 초기화 함수 호출
                    projScript.Initialize(dir, statManager.Attack);
                    UtillLogRemove.Log("쓰로머가 파편을 던졌습니다");
                }
            }

            // 0.2초 대기, 실패시 취소
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: this.GetCancellationTokenOnDestroy());

            // 만약 애니메이션이 있으면, 대기로 변경
            if (anim != null) anim.SetState(AllState.Idle);

            // 쿨타임에 - 0.5초 빼고 저장
            float debugCooldown = cooldown - 0.5f;

            // 만약 디버그 쿨타임이 0보다 크면
            if (debugCooldown > 0)
            {
                // 대기
                await UniTask.Delay(TimeSpan.FromSeconds(debugCooldown), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }
        catch (OperationCanceledException) { }
    }
}
