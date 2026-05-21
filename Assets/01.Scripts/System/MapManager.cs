using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; } // 싱글턴 선언

    private GameObject currentMapInstance; // 현재 사용하고 있는 맵 저장

    private AsyncOperationHandle<GameObject> mapHandle; // 어드레서블로 맵가져와 저장

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            gameObject.DestroySafe ();
        }
    }

    // 실제 맵과 플레이어 지점을 호출 하는 함수
    public async UniTask<Transform> SpawnSelectedMap(string mapAddress)
    {
        // 호출 된 맵이 있다면
        if (mapHandle.IsValid())
        {
            // 지워라
            Addressables.Release(mapHandle);
        }

        // 어드레서블을 사용하여 맵 불러오기
        mapHandle = Addressables.InstantiateAsync(mapAddress);

        // 가져오는 동안 기다리기
        await mapHandle;
        
        // 만약 맵가져오는 대 성공하면
        if (mapHandle.Status == AsyncOperationStatus.Succeeded)
        {   // 가져온 맵 변수 저장
            currentMapInstance = mapHandle.Result;

            // 맵에서 플레이어 스타트 지점 찾기
            Transform spawnPoint = currentMapInstance.transform.Find("StartPoint");

            // 스타트 지점이 없으면
            if (spawnPoint == null)
            {
                UtillLogRemove.Warning("맵 안에 'StartPoint'라는 이름의 빈 오브젝트가 없습니다!");
                return currentMapInstance.transform; // 반환
            }

            return spawnPoint; // 반환
        }
        else
        {
            // 로딩 실패시 에러 알림
            UtillLogRemove.Error($"맵 로드 실패! 주소 확인 요망: {mapAddress}");
            Addressables.Release(mapHandle); // 반환
            return null; // 널 반환
        }
    }
}
