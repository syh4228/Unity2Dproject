using UnityEngine;

public class StageEndTrigger : MonoBehaviour
{
    [Header("도착 지점 설정")]
    public string playerTag = "Player"; // 플레이어 태그 확인용

    // 2D 콜라이더에 트리거 설정이 된 오브젝트와 닿았을 때 실행
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 닿은 오브젝트의 태그가 플레이어라면
        if (collision.CompareTag(playerTag))
        {
            UtillLogRemove.Log("플레이어가 End 지점에 도착했습니다!");

            // GameManager를 통해 게임 클리어 및 스코어 UI 호출
            if (GameManager.Instance != null)
            {
                // 게임 상태를 스코어로 변경
                GameManager.Instance.GameClear();

                // 도착 지점 중복 터치를 막기 위해 본인 콜라이더 비활성화 (선택 사항)
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
}
