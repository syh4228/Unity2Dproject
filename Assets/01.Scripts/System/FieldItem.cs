using UnityEngine;

public class FieldItem : MonoBehaviour
{
    public string ItemId; // 데이터와 연결될 명찰

    // 스포너가 아이템을 생성할 때 아이디를 주입해주는 함수
    public void Setup(string id)
    {
        ItemId = id;
    }
}
