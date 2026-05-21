using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject enemyPrefab;  // 소환할 적 프리팹
    public float spawnInterval = 5f; // 리스폰 쿨타임

    [Header("스폰 거리 설정 (플레이어 기준)")]
    public float minSpawnDistance = 12f; // 최소 거리
    public float maxSpawnDistance = 18f; // 최대 거리

    private Transform playerTransform; // 플레이어 위치 받기
    private float spawnTimer = 0f;     // 스폰 타이머

    private void Start()
    {
        // 플레이어 컴포넌트 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        // 만약 플레이어가 있으면
        if (playerObj != null)
        {
            // 트랜스폼 받아오기
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        // 게임매니저가 있고, 게임매니저에서 전투중이 아닐때
        if (GameManager.Instance != null && GameManager.Instance.IsBattleActive == false)
        {
            return;
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
            SpawnEnemy();    // 적 스폰
            spawnTimer = 0f; // 타이머 초기화
        }
    }

    // 적 소환 함수
    private void SpawnEnemy()
    {
        // 만약 프리팹이 없으면
        if (enemyPrefab == null)
        {
            return;
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

        // 적 생성
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}
