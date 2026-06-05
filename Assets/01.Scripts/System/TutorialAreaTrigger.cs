using UnityEngine;

public class TutorialAreaTrigger : MonoBehaviour
{
    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 트리거에 닿은 대상이 플레이어라면
        if (collision.CompareTag("Player"))
        {
            // 2. 플레이어한테 붙어있는 웨이브 스포너를 바로 가져옵니다!
            EnemyWaveSpawner spawner = collision.GetComponent<EnemyWaveSpawner>();

            if (spawner != null)
            {
                spawner.isTutorialArea = true; // 웨이브 멈춤
                UtillLogRemove.Log("튜토리얼 구역 진입: 웨이브 생성 중지!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 1. 플레이어가 트리거 밖으로 나가면
        if (collision.CompareTag("Player"))
        {
            // 2. 다시 플레이어의 스포너를 가져와서
            EnemyWaveSpawner spawner = collision.GetComponent<EnemyWaveSpawner>();

            if (spawner != null)
            {
                spawner.isTutorialArea = false; // 웨이브 재개
                UtillLogRemove.Log("튜토리얼 구역 이탈: 웨이브 생성 재개!");
            }
        }
    }
    */
}
