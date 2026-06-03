using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject zombiePrefab; // 생성할 좀비
    public Transform spawnPoint;    // 생성 위치

    private bool isTriggered = false; // 트리거 작동 여부

    // 좀비 소환 카운터 용 변수
    private static int totalZombieCount = 0;

    // 트리거 작동 함수
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 부딪힌 대상이 플레이어 태그를 가지고 있으면
        if (collision.CompareTag("Player") == true)
        {
            // 트리거가 false면
            if (isTriggered == false)
            {
                // 트리거 투르로 변경
                isTriggered = true;

                // 좀비 소환 함수 호출
                SpawnZombie();
            }
        }
    }

    private void SpawnZombie()
    {
        // 좀비 프리팹, 스폰위치 정보를 받아 좀비 오브젝트 소환
        GameObject obj = Instantiate(zombiePrefab, spawnPoint.position, Quaternion.identity);

        // 하이라키에서 Enemys라는 오브젝트 찾아 저장
        GameObject enemyFolder = GameObject.Find("Enemys");

        // 오브젝트가 없으면
        if (enemyFolder == null)
        {
            // 만들어서 저장
            enemyFolder = new GameObject("Enemys");
        }

        // 빈오브젝트에 자식으로 프리팹 넣기
        obj.transform.SetParent(enemyFolder.transform);

        // 좀비 카운트 증가
        totalZombieCount = totalZombieCount + 1;

        // 좀비의 이름에 카운트를 붙여서 저장
        obj.name = "Zombie_" + totalZombieCount;

        UtillLogRemove.Log("배치형 스포너 작동! " + obj.name + " 소환 완료.");
    }
}
