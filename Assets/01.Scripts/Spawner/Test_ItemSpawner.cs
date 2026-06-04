using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Test_ItemSpawner : MonoBehaviour
{
    [Header("아이템 드롭 위치")]
    [SerializeField] private Transform dropPoint; // 아이템이 떨어질 위치 (플레이어 근처)

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) SpawnItemById("Weapon_PT_1");

        if (Input.GetKeyDown(KeyCode.F2)) SpawnItemById("Weapon_PT2_2");

        if (Input.GetKeyDown(KeyCode.F3)) SpawnItemById("Weapon_R0_3");

        if (Input.GetKeyDown(KeyCode.F4)) SpawnItemById("Weapon_SG_4");

        if (Input.GetKeyDown(KeyCode.F5)) SpawnItemById("Weapon_MG_5");

        if (Input.GetKeyDown(KeyCode.F6)) SpawnItemById("Weapon_SR_6");

        if (Input.GetKeyDown(KeyCode.F7)) SpawnItemById("Weapon_GN_7");

        if (Input.GetKeyDown(KeyCode.F8)) SpawnItemById("Weapon_HK_8");

        if (Input.GetKeyDown(KeyCode.F9)) SpawnItemById("Weapon_MD_9");

        if (Input.GetKeyDown(KeyCode.F10)) SpawnItemById("Weapon_AD_10");

        if (Input.GetKeyDown(KeyCode.F11)) SpawnItemById("Weapon_AC_11");
    }

    // 아이디를 받아서 JSON에서 경로를 찾아 소환하는 함수
    private void SpawnItemById(string itemId)
    {
        if (GameDataManager.Instance == null) return;

        // JSON에서 해당 아이템의 데이터 뭉치 가져오기
        WeaponData data = GameDataManager.Instance.GetWeaponData(itemId);

        if (data == null || string.IsNullOrEmpty(data.PrefabPath))
        {
            UtillLogRemove.Warning($"[Test] {itemId} 데이터를 찾을 수 없거나 PrefabPath가 비어있습니다.");
            return;
        }

        SpawnAsync(data).Forget();

    }

    private async UniTaskVoid SpawnAsync(WeaponData data)
    {
        Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position + new Vector3(1.5f, 0, 0);

        // 데이터에 적힌 경로(PrefabPath)를 이용해 Resources 폴더에서 프리팹 로드
        GameObject realItem = await ResourceManager.Inst.InstantiateAsync(data.PrefabPath, null);

        if (realItem != null)
        {
            // 위치 배치
            realItem.transform.position = spawnPos;

            // 필드 아이템 세팅 (없으면 추가)
            FieldItem fieldItem = realItem.GetComponent<FieldItem>() ?? realItem.AddComponent<FieldItem>();
            fieldItem.Setup(data.Id);

            // 부모 해제
            realItem.transform.SetParent(null);

            UtillLogRemove.Log($"[Test 치트] {data.Name} 어드레서블 소환 완료!");
        }
        else
        {
            UtillLogRemove.Error($"[Test] 프리팹 생성 실패! 어드레서블 주소({data.PrefabPath})를 확인하세요.");
        }
    }
}
