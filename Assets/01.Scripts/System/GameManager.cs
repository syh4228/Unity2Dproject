using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using static UnityEngine.Rendering.VolumeComponent;

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
    private string selectedCampaign; // 선택 캠페인 저장
    private int selectedChapter; // 선택 챕터 저장
    private int selectedDifficulty; // 선택 난이도 저장
    private int selectedCharacterIndex; // 캐릭터 고유 번호 저장

    public bool IsBattleActive // 실제 게임 플레이 중인지 확인(전투)
    {
        get; private set; // 외부로 알리기
    }


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

    private void Start()
    {
        // 로딩 프로세스함수 호출
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
                ChangeState(GameState.Lobby); // 로비 UI로 변환
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
        ChangeState(GameState.Loading); // 로딩UI로 변환
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

        if (SpawnPoint != null) // 만약 스타지 지점이 있으면
        {
            // 캐릭터매니저에서 선택된 캐릭터 생성
            CharacterManager.Instance.SpawnSelectedCharacter(selectedCharacterIndex, SpawnPoint);
        }

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
}
