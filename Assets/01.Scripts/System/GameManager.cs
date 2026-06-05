using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public enum GameState // 게임 현재 상태
{
    Start,
    Main,
    Lobby,
    Loading,
    Battle,
    Score
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // 싱글턴 선언

    [Header("게임 상태")]
    public GameState currentState; // 현재 게임 상태 저장

    [Header("연결 데이터")]
    private string currentMapName; // (캠페인 + 챕터)  = 실제 맵 주소
    private string selectedCampaign = ""; // 선택 캠페인 저장
    private int selectedChapter = -1; // 선택 챕터 저장
    private int selectedCharacterIndex = -1; // 캐릭터 고유 번호 저장

    public bool IsBattleActive // 실제 게임 플레이 중인지 확인(전투)
    {
        get; private set; // 외부로 알리기
    }
    
    // 저장한 선택 난이도 get, set을로 알려주기
    public int selectedDifficulty { get; private set; } = -1;

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

    private async void Start()
    {
        await UniTask.Yield(); // 대기
        StartLoadingProcess().Forget();
    }

    // 스타트 로딩 프로세스 함수
    private async UniTaskVoid StartLoadingProcess()
    {
        // UI매니저가 있으면
        if (UIManager.Instance != null)
        {
            // 스타트UI 매니저 가져오기
            StartUIManager startUI = UIManager.Instance.GetStartUI();

            if (startUI != null) // 있으면
            {
                // 스타트로딩 함수 호출
                await startUI.StartLoading();
            }
            else // 없으면
            {
                UtillLogRemove.Warning("StartUIManager가 연결되지 않았습니다!");
            }
        }
        else // 없으면
        {
            UtillLogRemove.Warning("UIManager 인스턴스를 찾을 수 없습니다!");
        }

        // 로딩이 완료되면 메인으로 이동합니다.
        ChangeState(GameState.Main);
    }

    private void Update()
    {
        if (currentState == GameState.Start) // 스타트UI 상태에서
        {
            if (Input.GetKeyUp(KeyCode.Return)) // 엔터키 누르면
            {
               ChangeState(GameState.Main); // 메인UI으로 변환
            }
        }

        if (currentState == GameState.Score) // 스코어UI 상태에서
        {
            if (Input.GetKeyUp(KeyCode.Return)) // 엔터키 누르면
            {
                ChangeState(GameState.Lobby); // 로비 UI로 변환
            }
        }

        if (currentState == GameState.Battle) // 배틀 씬 상태에서
        {
            if (Input.GetKeyUp(KeyCode.Escape)) // esc 누르면
            {
                // UI매니저에 팝업 켜져있는지 물어보고
                bool isPopupActive = UIManager.Instance.IsExitPopupActive();
                // UI매니저에 팝업을 켜져있으면 끄고, 꺼져있으면 키라고 알림
                UIManager.Instance.ShowExitpopup(!isPopupActive);
            }

            // UI매니저에 팝업이 켜져있는지 묻고
            if (UIManager.Instance.IsExitPopupActive())
            {
                // 엔터를 입력하면
                if (Input.GetKeyUp(KeyCode.Return))
                {
                    // UI매니저에 팝업 닫으라고 알림
                    UIManager.Instance.ShowExitpopup(false);
                    // 게임상태 로비로 전환
                    ChangeState(GameState.Lobby) ;
                }
            }
        }
    }

    public void ChangeState(GameState newstate) // 상태 변환 함수
    {
        currentState = newstate; // 현재 상태 저장

        // 현재 상태가 배틀이면 저장
        IsBattleActive = (currentState == GameState.Battle);

        switch (currentState)
        {
            case GameState.Start: // 스타트 상태시
                UIManager.Instance.CallStartUI(); // 스타트 UI 호출
                break;

            case GameState.Main: // 메인 상태시
                UIManager.Instance.CallMainUI(); // 메인 UI 호출
                break;

            case GameState.Lobby: // 로비 상태시
                ClearAllBattleRemnants(); // 로비UI로 이동전에 전투씬 청소
                UIManager.Instance.CallLobbyUI(); // 로비 UI 호출
                break;

            case GameState.Loading: // 로딩 상태시
                LoadingRoutineAsync().Forget(); // 로딩 UI 호출
                break;

            case GameState.Battle: // 배틀 상태시
                UIManager.Instance.CallBattleUI(); // 배틀 UI 호출
                break;

            case GameState.Score:
                UIManager.Instance.CallscoreUI();
                break;
        }
    }
    
    public void BtnClick_Play() // 플레이 버튼 이벤트 함수
    { 
        ChangeState(GameState.Lobby); // 로비UI로 변환
    }

    public void BtnClick_BackToMain()  // 뒤로가기 버튼 이벤트 함수
    {
        ChangeState(GameState.Main); // 메인UI로 변환
    }

    public void BtnClick_GameStart() // 게임 시작 버튼 이벤트 함수
    {
        // 캠페인,챕터,난이도,캐릭터중 하나라도 안고르면
        if (string.IsNullOrEmpty(selectedCampaign) || selectedChapter == -1 || selectedDifficulty == -1 || selectedCharacterIndex == -1)
        {
            UtillLogRemove.Warning("캠페인, 챕터, 난이도, 캐릭터를 모두 선택해야 게임을 시작할 수 있습니다!");

            if (UIManager.Instance != null) // ui매니저 있으면
            {
                // ui매니저에 경고창 이벤트 알림 함수 호출
                UIManager.Instance.ShowLobbyWarning();
            }

            return; // 반환
        }

        ChangeState(GameState.Loading); ; // 로딩UI로 변환
    }

    public void BtnClick_PlayerDie() // 플레이어 죽음 이벤트 함수
    { 
        ChangeState(GameState.Score); // 스코어UI로 변환
    }

    public void BtnClick_ExitGame()  // 나가기 버튼 이벤트 함수
    { 
        Application.Quit(); // 게임 종료
    }

    // 로딩 UI 게임 맵, 플레이어 준비 함수
    private async UniTaskVoid LoadingRoutineAsync()
    {
        // UI매니저에서 로딩UI 함수 호출
        UIManager.Instance.CallLoadingUI();
        // 대기
        await UniTask.Yield();

        currentMapName = ($"Map_{selectedCampaign}_{selectedChapter}");
        UtillLogRemove.Log($"조립된 맵 주소: {currentMapName}");

        Transform SpawnPoint = await MapManager.Instance.SpawnSelectedMap(currentMapName);

        if (SpawnPoint == null) // 만약 스타지 지점이 없으면
        {
            // 에러 알림
            UtillLogRemove.Error("맵 로드에 실패했습니다. 로비로 돌아갑니다.");

            ChangeState(GameState.Lobby); // 로비로 이동
            return; // 반환
        }

        // 캐릭터매니저에서 선택된 캐릭터 생성
        GameObject Player = CharacterManager.Instance.SpawnSelectedCharacter(selectedCharacterIndex, SpawnPoint);

        Camera.main.GetComponent<Player_CameraController>().SetTarget(Player.transform);

        if (UIManager.Instance.GetLodingUI() != null)
        {
            UIManager.Instance.GetLodingUI().SetDataLoaded();
        }
        // UniTask로 로딩 대기
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        // 배틀 씬 상태로 전환
        ChangeState(GameState.Battle);
    }

    public void SelectCampaign(string StageId)
    {
        selectedCampaign = StageId;
    }

    public void SelectChapter(int chapter)
    { 
        selectedChapter = chapter;
    }

    public void SetDifficulty(int diff)
    {
        selectedDifficulty = diff;
    }

    public void SetCharacter(int Index) 
    {
        selectedCharacterIndex = Index; 
    }

    // 배틀씬 청소 함수
    private void ClearAllBattleRemnants()
    {
        // 맵 매니저가 있으면
        if (MapManager.Instance != null)
        {
            // 맵매니저에서 맵정리 함수 호출
            MapManager.Instance.ClearMap();
        }

        // 만약 캐릭터 매니저가 있으면
        if (CharacterManager.Instance != null)
        {
            // 캐릭터 매니저에서 캐릭터 정리 함수 호출
            CharacterManager.Instance.ClearCharacter();
        }

        // 적 스탯매니저를 컴포넌트를 달고 있으면 찾아서 배열로 저장
        Enemy_StatManager[] remainingEnemies = FindObjectsByType<Enemy_StatManager>(FindObjectsSortMode.None);

        foreach (var enemy in remainingEnemies) // 배열안의 적을 하나씩 꺼내서 확인
        {
            if (enemy != null) // 적이 있으면
            {
                Destroy(enemy.gameObject); // 삭제
            }
        }

        // 총알매니저 컴포넌트를 달고 있으면 찾아서 배열로 저장
        BulletManager[] remainingBullets = FindObjectsByType<BulletManager>(FindObjectsSortMode.None);

        foreach (var bullet in remainingBullets) // 배열안에 총알을 하나씩 꺼내서 확인
        {
            if (bullet != null) // 총알이 있으면
            {
                bullet.gameObject.SetActive(false); // 비활성화
            }
        }


        UtillLogRemove.Log(" 전투 구역의 모든 잔해(플레이어, 맵, 풀링된 적) 청소 완료!");
    }

    public void GameClear()
    {
        UtillLogRemove.Log("스테이지 클리어! 스코어 화면으로 이동합니다.");
        ChangeState(GameState.Score); // 스코어 UI로 상태 변환
    }
}
