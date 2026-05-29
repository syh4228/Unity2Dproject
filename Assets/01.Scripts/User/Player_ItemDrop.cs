using UnityEngine;

public class Player_ItemDrop : MonoBehaviour
{
    // 인벤토리 연결
    [SerializeField] private Player_InventoryManager inventoryManager;

    // 법위안에 들어온 아이템 저장 변수
    private FieldItem _nearbyItem = null;

    private void Update()
    {
        // 매 프레임마다 V키가 눌렸는지 감시합니다.
        if (Input.GetKeyDown(KeyCode.V))
        {
            TryPickUp(); // V키를 누르면 줍기 로직 실행
        }
    }

    public void TryPickUp()
    {
        // 범위에 아이템 있고, 인벤토리 매니저 있으면
        if (_nearbyItem != null && inventoryManager != null)
        {
            // 인벤토리 매니저 아이템 줍기 함수 호출
            inventoryManager.PickUpItem(_nearbyItem.ItemId);

            UtillLogRemove.Log($"아이템 획득: {_nearbyItem.ItemId}");

            // 주운 아이템 오브젝트 파괴
            Destroy(_nearbyItem.gameObject);

            // 아이템 저장된거 삭제
            _nearbyItem = null;
        }
    }

    // 트리거에 범위에 들어왔으면
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 들어온 오브젝트의 태그가 "Item"인지 확인
        if (collision.CompareTag("Item"))
        {
            // 그 아이템에 붙어있는 FieldItem 스크립트를 가져와서 기억해둠
            _nearbyItem = collision.GetComponent<FieldItem>();
        }
    }

    // 트리거 범위에서 벗어났으면
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            // 내가 방금 나간 아이템을 확인
            FieldItem item = collision.GetComponent<FieldItem>();

            // 내가 기억하고 있던 아이템과 같다면, 범위 밖으로 나갔으므로 기억 삭제
            if (_nearbyItem == item)
            {
                _nearbyItem = null;
            }
        }
    }
}
