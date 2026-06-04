using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    // 스폰 모드 선택지(고정, 랜덤), 
    public ESpawnMode SpawnMode = ESpawnMode.FixedItem; // 기본은 고정

    [Header("고정 스폰일 경우 ID 입력")]
    public string FixedItemId; // 고정 스폰으로 받을 아이템 저장

    private void Start()
    {
        // 스폰 된 아이템 id 저장
        string finalItemId = DetermineItemId();

        // 아이템의 id가 없다면
        if (string.IsNullOrEmpty(finalItemId))
        {
            Destroy(gameObject); // 스포너 삭제
            return; // 반환
        }

        //  게임데이터 매니저에서 아이템 Id 가져오기
        var itemData = GameDataManager.Instance.GetWeaponData(finalItemId);
        
        // 게임 데이터가 있고, 프리팹 경로도 있으면
        if (itemData != null && !string.IsNullOrEmpty(itemData.PrefabPath))
        {
            // Resources 폴더에서 프리팹 원본 파일을 가져오기
            GameObject prefab = Resources.Load<GameObject>(itemData.PrefabPath);

            // 프리팹이 있으면
            if (prefab != null)
            {
                // 프리팹 스포너 위치에 생성
                GameObject realItem = Instantiate(prefab, transform.position, Quaternion.identity);

                // 생성된 아이템에 FieldItem 컴포넌트 있는지 확인
                FieldItem fieldItem = realItem.GetComponent<FieldItem>();

                // 컴포넌트가 없으면
                if (fieldItem == null) 
                {
                    // 컴포넌트 달아주기
                    fieldItem = realItem.AddComponent<FieldItem>();
                }

                // 아이템 아이디 컴포넌트에 저장
                fieldItem.Setup(finalItemId);
            }
            else
            {
                UtillLogRemove.Error($"프리팹을 찾을 수 없습니다: {itemData.PrefabPath}");
            }
        }

        // 스포너 삭제
        Destroy(gameObject);
    }

    // 스폰 모드에 따라 어떤 아이템 소환 함수
    private string DetermineItemId()
    {
        // 게임매니저에서 웨폰데이터 리스트 가져오기
        var allItems = GameDataManager.Instance.WeaponDataList.Values.ToList();

        // 웨폰데이터 리스트 저장
        List<WeaponData> candidateList = new List<WeaponData>();

        switch (SpawnMode) // 스폰모드 확인
        {
            case ESpawnMode.FixedItem: // 고정 스폰이면
                return FixedItemId; // 적어둔 ID 그대로 반환

            case ESpawnMode.RandomGun: // 랜덤 총기 스폰이면
                
                foreach (var item in allItems) // 하나씩 꺼내서
                {
                    if (item.UseType == "Gun") // 타입이 건이면
                    {
                        candidateList.Add(item); // 리스트에 저장
                    }
                }
                break;

            case ESpawnMode.RandomConsumable: // 랜덤 소모품 스폰 이면
                                              
                foreach (var item in allItems) 
                {
                    // 타입이 붐이거나, 타입이 힐인데 HK이 아니면
                    if (item.UseType == "Boom" || (item.UseType == "Heel" && !item.Id.Contains("HK")))
                    {
                        candidateList.Add(item); // 리스트에 저장
                    }
                }
                break;
        }

        if (candidateList.Count > 0) // 리스트에 하나라도 있으면
        {
            // 랜덤으로 하나 뽑아 저장
            int randomIndex = Random.Range(0, candidateList.Count);
            return candidateList[randomIndex].Id; // 아이디 저장
        }

        return string.Empty; // 없으면 빈 글자 반환
    }
}
