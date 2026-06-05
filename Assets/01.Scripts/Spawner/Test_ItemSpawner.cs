using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Test_ItemSpawner : MonoBehaviour
{
    [Header("아이템 드롭 위치")]
    [SerializeField] private Transform dropPoint; // 아이템이 떨어질 위치 (플레이어 근처)

    [Header("총기 설정")]
    [SerializeField] private float gunFirstDelay = 10f; // 처음엔 이 시간 뒤에
    [SerializeField] private float gunInterval = 30f; // 그 다음부턴 이 주기로

    [Header("기타 설정")]
    [SerializeField] private float consumablesInterval = 15f; // 소모품 주기
    [SerializeField] private float healsInterval = 20f; // 힐템 주기

    private void Start()
    {
        GunDropRoutine().Forget();
        ConsumableDropRoutine().Forget();
        HealDropRoutine().Forget();
    }

    private async UniTaskVoid GunDropRoutine()
    {
        // 총기는 첫 지연시간 적용
        await UniTask.Delay(System.TimeSpan.FromSeconds(gunFirstDelay));
        while (true)
        {
            SpawnRandomItem("Gun");
            await UniTask.Delay(System.TimeSpan.FromSeconds(gunInterval));
        }
    }

    private async UniTaskVoid ConsumableDropRoutine()
    {
        while (true)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(consumablesInterval));
            SpawnRandomItem("Consumable");
        }
    }

    private async UniTaskVoid HealDropRoutine()
    {
        while (true)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(healsInterval));
            SpawnRandomItem("Heel");
        }
    }

    private void SpawnRandomItem(string type)
    {
        if (GameDataManager.Instance == null) return;

        var allItems = GameDataManager.Instance.WeaponDataList.Values.ToList();
        List<WeaponData> targetList = new List<WeaponData>();

        foreach (var item in allItems)
        {
            if (type == "Gun" && item.UseType == "Gun") targetList.Add(item);
            else if (type == "Consumable" && (item.UseType == "Boom" || item.UseType == "AD" || item.UseType == "MD")) targetList.Add(item);
            else if (type == "Heel" && item.UseType == "Heel") targetList.Add(item);
        }

        if (targetList.Count > 0)
        {
            WeaponData selectedData = targetList[Random.Range(0, targetList.Count)];
            SpawnAsync(selectedData).Forget();
        }
    }

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
            realItem.transform.position = dropPoint != null ? dropPoint.position : transform.position;
            FieldItem fieldItem = realItem.GetComponent<FieldItem>() ?? realItem.AddComponent<FieldItem>();
            fieldItem.Setup(data.Id);
            realItem.transform.SetParent(null);
            UtillLogRemove.Log($"[Auto Drop] {data.Name} 소환됨!");
        }
    }
}
