using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

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

    public GameState currentState; // 현재 게임 상태 저장 변수

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
        ChangeState(GameState.Start); // 시작시 겜이 시작 상태로 바꾸기
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
            if (!Input.GetKeyUp(KeyCode.Return)) // 엔터키 누르면
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
        UIManager.Instance.CallLoadingUI(); // 로딩 UI 호출

        await UniTask.Yield(); // Unitask 실행

        // 맵 매니저에서 플레이어 스타트 위치 정보 가져오기
        Transform spawnPoint = MapManager.Instance.SpawnSelectedMap();
        if (spawnPoint != null) // 만약 스폰 포인트 있으면
        {
            // 캐릭터 매니저에서 캐릭터 정보 가져오기
            CharacterManager.Instance.SpawnSelectedCharacter(spawnPoint);
        }

        // Unitask로 대기 시간 실행
        await UniTask.Delay(TimeSpan.FromSeconds(2f));

        // 배틀씬으로 전환
        ChangeState(GameState.Battle);
    }
}
