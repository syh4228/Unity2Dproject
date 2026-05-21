using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject enemyPrefab;  // 소환할 적 프리팹
    public float spawnInterval = 5f; // 리스폰 쿨타임
    [SerializeField] private int poolSize = 10; // 미리 만둘어둘 숫자

    [Header("스폰 거리 설정 (플레이어 기준)")]
    public float minSpawnDistance = 12f; // 최소 거리
    public float maxSpawnDistance = 18f; // 최대 거리

    private Transform playerTransform; // 플레이어 위치 받기
    private float spawnTimer = 0f;     // 스폰 타이머

    private List<GameObject> enemyPool; // 생성된 적을 리스트 저장

    private void Start()
    {
        enemyPool = new List<GameObject>(); // 생성된 적을 리스트에 담을 변수

        for (int i = 0; i < poolSize; i++) // 리스트에서 하나씩 꺼내 확인
        {
            GameObject enemy = Instantiate(enemyPrefab); // 프리펩 가져오기
            enemy.SetActive(false); // 비활성화
            enemyPool.Add(enemy); // 리스트에 저장
        }
    }

    private void Update()
    {
        // 게임매니저가 있고, 게임매니저에서 전투중이 아닐때
        if (GameManager.Instance != null && GameManager.Instance.IsBattleActive == false)
        {
            return; // 반환
        }

        // 만약 플레이어 트랜스폼이 없으면
        if (playerTransform == null)
        {
            // 플레이어 태그 가진 오브젝트 찾기
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null) // 있으면
            {
                // 트랜스폼 정보 가져오기
                playerTransform = playerObj.transform; 
            }
            else // 없으면
            {
                return; // 반환
            }
        }

        // 스폰 쿨타임 
        spawnTimer = spawnTimer + Time.deltaTime;

        // 스폰 쿨타임 완료시
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemyFromPool();    // 적 스폰 함수 호출
            spawnTimer = 0f; // 타이머 초기화
        }
    }

    // 적 소환 함수
    private void SpawnEnemyFromPool()
    {
        // 만약 프리팹이 없으면
        if (enemyPrefab == null)
        {
            return;
        }

        // 
        GameObject targetEnemy = null;

        foreach (GameObject enemy in enemyPool) // 리스트에 있은 적 꺼내기
        {
            if (enemy.activeSelf == false) // 만약 비활성화라면
            {
                targetEnemy = enemy; // 적 저장
                break;
            }
        }

        if (targetEnemy == null) // 만약 저장된 적이 없으면
        {
            targetEnemy = Instantiate(enemyPrefab); // 프리팹에서 가져오기
            enemyPool.Add(targetEnemy); // 리스트에 추가
        }

        // 나오는 방향 랜덤 설정
        int randomDirection = Random.Range(0, 2);
        // 스폰
        float spawnDirectionX = 1f; // 스폰 x축 기본 값

        if (randomDirection == 0) // 0 이면
        {
            spawnDirectionX = -1f; // 플레이어의 왼쪽 영역으로 결정
        }
        else // 아니면
        {
            spawnDirectionX = 1f;  // 플레이어의 오른쪽 영역으로 결정
        }

        // 스폰 랜덤 위치 계산
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

        // [최종 좌표 계산] 
        // 스폰 위치 x 축 값 계산
        // 스폰 위치 Y 값 => 1 고정
        float finalSpawnX = playerTransform.position.x + (spawnDirectionX * randomDistance);
        float finalSpawnY = playerTransform.position.y + 1f;

        // 스폰위치 최종 계산
        Vector2 spawnPosition = new Vector2(finalSpawnX, finalSpawnY);

        // 적 스폰 위치로 이동
        targetEnemy.transform.position = spawnPosition;

        // 적 스탯 매니저에서 컴포넌트 가져오기
        Enemy_StatManager stat = targetEnemy.GetComponent<Enemy_StatManager>();

        if (stat != null) // 스탯이 있으면
        {
            stat.ResetEnemy(); // 적 스탯 적용
        }

        targetEnemy.SetActive(true); // 적 활성화
    }
}
