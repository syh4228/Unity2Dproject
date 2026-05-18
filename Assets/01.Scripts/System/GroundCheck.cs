using UnityEngine;
using System;

public class GroundCheck : MonoBehaviour
{
    // 지면에 붙어 있는지 체크
    public event Action<bool> GroundTriggeredEvent;

    // 다른 2D 콜라이더가 내 2D 트리거 영역 안에 있으면 작동하는 함수
    private void OnTriggerStay2D(Collider2D other)
    {
        // 만약 연결된 이벤트가 있으면
        if (GroundTriggeredEvent != null)
        {
            // true로 변경
            GroundTriggeredEvent.Invoke(true);
        }
    }

    // 2D 트리거 영역 안에 있던 다른 2D 콜라이더가가 영역 밖으로 벗어날때 한번 실행되는 함수
    private void OnTriggerExit2D(Collider2D collision)
    {
        // 만약 연결된 이벤트
        if (GroundTriggeredEvent != null)
        {  
            // false로 변경
            GroundTriggeredEvent.Invoke(false);
        }
    }
}
