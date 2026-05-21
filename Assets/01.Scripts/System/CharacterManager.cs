using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; } // 싱글턴 선언

    [Header("캐릭터 컴포넌트")]
    [SerializeField] private GameObject[] characterPrefabs; // 캐릭터 프리팹 배열로 연결

    private int selectedIndex = 0; // 선택된 캐릭터 번호부여 변수
    private GameObject currentCharact; // 현재 캐릭터

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            gameObject.DestroySafe();
        }
    }

    // 로비UI에서 캐릭터 선택시 캐릭터 저장 함수
    public void SelectCharacter(int index)
    {
        selectedIndex = index;
        UtillLogRemove.Log($"캐릭터 선택: {characterPrefabs[index].name}");
    }

    // 로딩UI에서 맵의 스타트 지점에 캐릭터 호출 함수
    public GameObject SpawnSelectedCharacter(int characterIndex, Transform spawnPoint)
    {
        // 현재 캐릭터가 있으면
        if (currentCharact != null)
        {
            // 현재 캐릭터 삭제
            currentCharact.DestroySafe();
        }

        // 프리팹에서 선택 캐릭터 가져와 스타트지점에 생성
        currentCharact = Instantiate(characterPrefabs[characterIndex], spawnPoint.position, spawnPoint.rotation);

        UtillLogRemove.Log("캐릭터 소환 완료!");

        return currentCharact;
    }

    // 캐릭터 청소 함수
    public void ClearCharacter() 
    {
        if (currentCharact != null) // 캐릭터가 있다면
        {
            currentCharact.DestroySafe(); // 플레이어 파괴
            currentCharact = null;
            UtillLogRemove.Log("기존 플레이어 오브젝트가 파괴되었습니다.");
        }
    }
}
