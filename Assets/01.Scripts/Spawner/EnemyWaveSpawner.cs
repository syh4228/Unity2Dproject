using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    [Header("상태 체크")]
    public bool isTutorialArea = false; // 튜토리얼 구역인지 확인
    public bool isSafeHouse = false;    // 세이프 하우스 구역인지 확인

    [Header("스폰 범위 설정")]
    public float noSpawnRadius = 15f; // 스폰 방지 구역
    public float maxActiveRadius = 40f; // 적 붕 뜨기 방지 범위

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
            // 특수 좀비 스폰 함수 호출
            HandleSpecialZombieSpawn();
            // 타이머 다시 0으로 초기화
            specialWaveTimer = 0f;
        }

        // 일반 좀비 웨이브 타이머가 일반 좀비 웨이브 시간 주기 이상이면
        if (normalWaveTimer >= normalWaveInterval)
        {
            // 일반 좀비 스폰 함수 호출
            HandleWaveSpawn();
            // 타이머 다시 0으로 초기화
            normalWaveTimer = 0f;
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
        // 특수좀비 숫자 카운터가 최대 숫자보다 적으면
        if (activeSpecialZombies.Count < maxSpecialZombies)
        {
            // 특수 좀비 배열이 비어있지 않다면
            if (specialZombiePrefabs.Length > 0)
            {
                // 특수 좀비 중에서 랜덤으로 1나 골라서 저장
                int randomIndex = Random.Range(0, specialZombiePrefabs.Length);

                // 골라진 특수 좀비 프리팹 저장
                GameObject selectedPrefab = specialZombiePrefabs[randomIndex];

                // 스폰 위치 계산
                Vector2 spawnPos = GetValidSpawnPosition();

                // 특수좀비 생성
                GameObject newSpecialZombie = SpawnZombieSetup(selectedPrefab, spawnPos);

                // 생성된 특수 좀비 정보 적 스탯 매니저에서 가져오기
                Enemy_StatManager stat = newSpecialZombie.GetComponent<Enemy_StatManager>();

                if (stat != null) // 스탯이 있으면
                {
                    // 적 id 가져와서 저장
                    string id = stat.enemyId;

                    // 데이터 매니저에서 몬스터 id에 맞는 데이터 가져와 저장
                    DNMonsterData data = GameDataManager.Instance.GetDNMonsterData(id);

                    if (data != null) // 데이터가 있으면
                    {
                        // 스탯 적용
                        stat.Initialize(data);
                    }
                    else
                    {
                        UtillLogRemove.Error("특수 좀비 ID [" + id + "]에 맞는 JSON 데이터를 찾지 못했습니다");
                    }

                    activeSpecialZombies.Add(stat); // 특수 좀비 리스트 추가
                    ForceAggro(newSpecialZombie); // 강제 어그로 함수 호출
                    UtillLogRemove.Log("특수 좀비 [" + stat.enemyName + "] 소환, 현재 특수 좀비 수: " + activeSpecialZombies.Count);
                }
            }
        }
    }

    // 일반 좀비 소환 함수
    private void HandleWaveSpawn()
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
            // 일반 좀비 프리팹 배열이 있다면
            if (normalZombiePrefabs.Length > 0) 
            {
                // 일반 좀비 프리팹 중 랜덤 선택
                int randomIndex = Random.Range(0, normalZombiePrefabs.Length);
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
                        // 스탯 주입
                        stat.Initialize(data);
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
        // 0도부터 360도 사이의 랜덤한 각도 저장
        float randomAngle = Random.Range(0f, 360f);
        // 유니티가 읽은 수있게 변환 저장
        float radian = randomAngle * Mathf.Deg2Rad;

        // 최소 범위(15)와 최대 범위(40) 사이의 랜덤한 거리 저장
        float randomDistance = Random.Range(noSpawnRadius, maxActiveRadius);

        // 내 위치 기준으로 랜덤 각도와 거리만큼 떨어진 X 좌표 계산 저장
        float spawnX = transform.position.x + (Mathf.Cos(radian) * randomDistance);
        // 내 위치 기준으로 랜덤 각도와 거리만큼 떨어진 y 좌표 계산 저장
        float spawnY = transform.position.y + (Mathf.Sin(radian) * randomDistance);

        return new Vector2(spawnX, spawnY); // 새 위치 반환
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
}
