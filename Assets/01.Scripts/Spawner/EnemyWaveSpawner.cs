using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("상태 체크")]
    public bool isTutorialArea = false; // 튜토리얼 구역인지 확인
    public bool isSafeHouse = false;    // 세이프 하우스 구역인지 확인

    [Header("스폰 범위 설정")]
    public float noSpawnRadius = 15f; // 스폰 방지 구역
    public float maxActiveRadius = 40f; // 적 붕 뜨기 방지 범위
    public float spawnHeight = 4f; // 범위 생성 높이

    [Header("일반 좀비 웨이브 설정")]
    public GameObject[] normalZombiePrefabs; // 일반 좀비 프리팹들
    public int maxNormalZombies = 30; // 일반 좀비 최대 유지 수
    public float normalWaveInterval = 120f; // 웨이브 주기
    private float normalWaveTimer = 0f; // 일반 좀비 웨이브 시간 측정용 타이머

    // 적 스탯 매니저에서 일반 좀비 정보를 받아와 리스트로 저장
    public List<Enemy_StatManager> activeNormalZombies = new List<Enemy_StatManager>();

    [Header("특수 좀비 설정")]
    public GameObject[] specialZombiePrefabs; // 특수 좀비 프리팹
    public int maxSpecialZombies = 4; // 특수 좀비 최대 유지 수
    public float specialWaveInterval = 20f; // 특수 좀비 소환 주기
    private float specialWaveTimer = 0f; // 특수 좀비 시간 측정용 타이머
    // 적 스텟 매니저에서 정보받아와 특수 좀비 리스트로 저장

    // 적 스탯 매니저에서 특수 좀비 정보 받아와 리스트로 저장
    public List<Enemy_StatManager> activeSpecialZombies = new List<Enemy_StatManager>();

    // 좀비 숫자 카운트
    public static int totalZombieCount = 0;

    private void Update()
    {
        // 튜토리얼 지역 이면
        if (isTutorialArea == true)
        {
            return;
        }

        // 세이프 하우스 지역이면
        if (isSafeHouse == true)
        {
            return;
        }

        // 일반 좀비 웨이브 타이머 시간 증가
        normalWaveTimer = normalWaveTimer + Time.deltaTime;
        // 특수 좀비 웨이브 타이머 시간 증가
        specialWaveTimer = specialWaveTimer + Time.deltaTime;

        // 좀비 위치 관리 함수 호출
        ManageZombieDistances();

        // 특수 좀비 웨이브 타이머가 특수 좀비 웨이브 시간 주기 이상이면
        if (specialWaveTimer >= specialWaveInterval)
        {
            // 웨이브 타이머에 웨이브 시간 주기만큼 빼기
            specialWaveTimer -= specialWaveInterval;
            // 특수 좀비 스폰 함수 호출
            HandleSpecialZombieSpawn();
        }

        // 일반 좀비 웨이브 타이머가 일반 좀비 웨이브 시간 주기 이상이면
        if (normalWaveTimer >= normalWaveInterval)
        {
            // 웨이브 타이머에 웨이브 시간 주기만큼 빼기
            normalWaveTimer = 0;
            // 일반 좀비 스폰 함수 호출
            HandleWaveSpawn().Forget();
        }
    }

    // 좀비 위치 관리 함수
    private void ManageZombieDistances()
    {
        // 좀비 타운터 숫자에 -1 씩해서 하나씨 꺼내서 확인
        for (int i = activeNormalZombies.Count - 1; i >= 0; i--)
        {
            // 적 스탯 매니저에서 일반 좀비 저장
            Enemy_StatManager normalZombie = activeNormalZombies[i];

            // 좀비가 없거나, 비활성화 상태면
            if (normalZombie == null || normalZombie.gameObject.activeSelf == false)
            {
                // 리스트에서 제거
                activeNormalZombies.RemoveAt(i);
                continue; // 아래는 무시하고 다시 하나 꺼내서 확인
            }

            // 내 위치와 좀비위치 저장
            float distance = Vector2.Distance(transform.position, normalZombie.transform.position);

            // 거리가 적 붕띄기 방지 범위 밖에 있으면
            if (distance > maxActiveRadius)
            {
                // 스폰 위치 가져오기 함수 호출하여 저장
                normalZombie.transform.position = GetValidSpawnPosition();
                // 강제 어그로 함수 호출
                ForceAggro(normalZombie.gameObject);

                UtillLogRemove.Log(normalZombie.gameObject.name + " 너무 멀어져서 텔레포트 및 어그로 리셋!");
            }
        }

        // 특수 좀비 카운터 숫자에서 -1 씩 해서 하나씩 꺼내서 확인
        for (int i = activeSpecialZombies.Count - 1; i >= 0; i--)
        {
            // 적 스탯 매니저에서 특수 좀비 정보 저장
            Enemy_StatManager specialZombie = activeSpecialZombies[i];

            // 좀비가 없고, 좀비가 비활성화 상태면
            if (specialZombie == null || specialZombie.gameObject.activeSelf == false)
            {
                // 특수 좀비 리스트에서 제거
                activeSpecialZombies.RemoveAt(i);
                continue; // 밑에 무시하고, 다시 - 1 꺼내서 실행
            }

            // 내 위치와 좀비 위치 확인후 저장
            float distance = Vector2.Distance(transform.position, specialZombie.transform.position);

            if (distance > maxActiveRadius) // 붕 뜨기 방지 범위 밖에 있으면
            {
                // 스폰 위치 가져오기 함수 호출
                specialZombie.transform.position = GetValidSpawnPosition();
                // 강제 어그로 함수 호출
                ForceAggro(specialZombie.gameObject);

                UtillLogRemove.Log("특수 좀비 " + specialZombie.gameObject.name + " 텔레포트 완료!");
            }
        }
    }

    // 특수 좀비 스폰 함수
    private void HandleSpecialZombieSpawn()
    {
        // 특수 좀빕 카운트가 최대 특수좀비 숫자 보다 크거나 같으면 리턴
        if (activeSpecialZombies.Count >= maxSpecialZombies) return;
        // 특수 좀비 프리펨 배열이 0이면 리턴
        if (specialZombiePrefabs.Length <= 0) return;

        // 특수 좀비 중 랜덤으로 1개 골라서 저장
        int randomIndex = UnityEngine.Random.Range(0, specialZombiePrefabs.Length);
        // 뽑인 특수좀비 프리펨 저장
        GameObject selectedPrefab = specialZombiePrefabs[randomIndex];
        // 프리펩 소환 위치 알아보는 함수 호출
        Vector2 spawnPos = GetValidSpawnPosition();

        // 특수 좀비 프리펩과 스폰위치 저장
        GameObject newSpecialZombie = SpawnZombieSetup(selectedPrefab, spawnPos);
        // 적 스탯 매니저에서 스탯 컴포넌트 가져와 저장
        Enemy_StatManager stat = newSpecialZombie.GetComponent<Enemy_StatManager>();

        if (stat != null) // 스탯이 있으면
        {
            string id = stat.enemyId; // id 저장
            // 게임데이터 매니저에서 맞는 id찾아 데이터 저장
            DNMonsterData data = GameDataManager.Instance.GetDNMonsterData(id);

            if (data != null) // 데이터 있으면
            {
                // 게임매니저에서 선택 난이도 가져와 저장
                int currentDiff = GameManager.Instance.selectedDifficulty;
                // 생성
                stat.Initialize(data, currentDiff);
            }
            else // 없으면
            {
                UtillLogRemove.Error("특수 좀비 ID [" + id + "]에 맞는 JSON 데이터를 찾지 못했습니다");
            }

            activeSpecialZombies.Add(stat); // 특수좀비 리스트에 추가
            ForceAggro(newSpecialZombie); // 어그로 함수 호풀
            UtillLogRemove.Log("특수 좀비 [" + stat.enemyName + "] 소환, 현재 수: " + activeSpecialZombies.Count);
        }
    }

    // 일반 좀비 소환 함수
    private async UniTaskVoid HandleWaveSpawn()
    {
        // 일반 좀비 최대 숫자에서 현재 있는 숫자 빼서 저장
        int neededZombies = maxNormalZombies - activeNormalZombies.Count;

        // 필요한 좀비 숫자가 0 보다 작으면
        if (neededZombies <= 0)
        {
            return; // 반환
        }

        // 필요한 숫자가 될때 까지 하나씩 꺼내서 확인
        for (int i = 0; i < neededZombies; i++)
        {
            // 0.3초 유니테스크 대기
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));

            // 일반 좀비 프리팹 배열이 있다면
            if (normalZombiePrefabs.Length > 0) 
            {
                // 일반 좀비 프리팹 중 랜덤 선택
                int randomIndex = UnityEngine.Random.Range(0, normalZombiePrefabs.Length);
                // 선택 된 일반 좀비 프리팹 저장
                GameObject selectedPrefab = normalZombiePrefabs[randomIndex];

                // 스폰 구역 저장
                Vector2 spawnPos = GetValidSpawnPosition();

                // 생성
                GameObject newZombie = SpawnZombieSetup(selectedPrefab, spawnPos);

                // 적스텟 매니저에서 일반 좀비 스텟 가져와 저장
                Enemy_StatManager stat = newZombie.GetComponent<Enemy_StatManager>();

                if (stat != null) // 스탯이 있으면
                {
                    // 적 스탯에서 id 가져와 저장
                    string id = stat.enemyId;

                    // 데이터 매니저에서 id에 맞는 정보 가져와 저장
                    DNMonsterData data = GameDataManager.Instance.GetDNMonsterData(id);

                    if (data != null) // 데이터가 있으면
                    {
                        // 게임매니저에서 선택한 난이도 정보를 받아서 저장
                        int currentDiff = GameManager.Instance.selectedDifficulty;
                        // 현재 난이도에 맞게 스텟 적용
                        stat.Initialize(data, currentDiff);
                    }
                    else
                    {
                        UtillLogRemove.Error("일반 좀비 ID [" + id + "]에 맞는 JSON 데이터를 찾지 못했습니다");
                    }

                    // 일반 좀비 리스트 추가
                    activeNormalZombies.Add(stat);
                    ForceAggro(newZombie); // 강제 어그로 함수 호출
                }
            }
        }

        UtillLogRemove.Log("일반 좀비 웨이브" + neededZombies + "마리 강제 소환됨.");
    }

    // 적 붕 뜨기 방지 구역 함수
    private Vector2 GetValidSpawnPosition()
    {
        //  30 까지 하나씩 꺼내서 확인
        for (int i = 0; i < 30; i++)
        {
            // 본인 위치 기준으로 x좌표 계산
            float spawnX = transform.position.x + UnityEngine.Random.Range(-maxActiveRadius, maxActiveRadius);
            // 본인 위치 기준으로 Y좌표 계산
            float spawnY = transform.position.y + UnityEngine.Random.Range(-spawnHeight / 2f, spawnHeight / 2f) + 0.8f;
            // x,y 좌표 저장
            Vector2 candidate = new Vector2(spawnX, spawnY);

            // 계산한 x 좌표 - 본인위치 값이 스폰 금지 구역 보다 크면
            if (Mathf.Abs(candidate.x - transform.position.x) < noSpawnRadius)
            {
                continue; // 다시 좌표 계산
            }

            return candidate; // 작으면 저장해서 반환
        }

        // 30번 실패 하면 본인 x위치에 스폰금지 구역을 더한 값에 1을 더해 좌표값 반환
        return new Vector2(transform.position.x + noSpawnRadius + 1f, transform.position.y + 0.8f);
    }

    // 강제 어그로 함수
    private void ForceAggro(GameObject zombieObj)
    {
        // 적 AI 매니저에서 컴포넌트 가져와 저장
        Enemy_AiManager ai = zombieObj.GetComponent<Enemy_AiManager>();

        if (ai != null) // ai가 있으면
        {
            // 감지범위 1000으로 증가
            ai.detectRange = 1000f;
        }
    }

    // 좀비 소환 함수
    private GameObject SpawnZombieSetup(GameObject prefab, Vector2 spawnPosition)
    {
        // 지정한 위치에 좀비 소환
        GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // 하이라키에서 Enemys 빈오브젝트 찾아서 저장
        GameObject enemyFolder = GameObject.Find("Enemys");

        // 빈오브젝트가 없으면
        if (enemyFolder == null)
        {
            // 빈오브젝트 Enemys로 생성 저장
            enemyFolder = new GameObject("Enemys");
        }

        // 소환 된 좀비를 enemy 빈오브젝트 자식으로 넣기
        obj.transform.SetParent(enemyFolder.transform);

        // 좀비 카운트 +1
        totalZombieCount = totalZombieCount + 1;

        // 좀비 이름 뒤에 카운트 추가
        obj.name = "Zombie_" + totalZombieCount;

        // 오브젝트 반환
        return obj;
    }

    // 기즈모로 범위 그려주는 함수
    private void OnDrawGizmos()
    {
        // 플레이어의 현재 위치를 중심으로 그립니다.
        Vector3 center = transform.position;

        // 적 붕띄기 방지 구역 (빨간색 직사각형)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(maxActiveRadius * 2f, spawnHeight, 0f));

        // 스폰 방지 구역 범위  (노란색 직사각형)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(noSpawnRadius * 2f, spawnHeight, 0f));
    }

}
