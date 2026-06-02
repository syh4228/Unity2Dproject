using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class Player_Controller : MonoBehaviour
{
    [Header("플레이어 설정")]
    [SerializeField] private float _moveSpeed = 2f; // 이동속도(기본 달리기)

    [Header("점프 및 물리")]
    [SerializeField] private float _jumpForce = 7f; // 점프 힘
    [SerializeField] private bool _isGrounded; // 지면에 닿았는지 확인

    [Header("컴포넌트")]
    [SerializeField] private Rigidbody2D Rigidbody_Player; // 리지드바디
    [SerializeField] private GroundCheck GroundCheckObject; // 지면체크 스크립트를 연결한 자식 오브젝트
    [SerializeField] private AnimationController AnimatorController; // 애니메이션 
    [SerializeField] private WeaponManager weaponManager; // 웨폰 매니저 연결
    [SerializeField] private Player_InventoryManager inventoryManager; // 인벤토리 매니저
    [SerializeField] private Player_Character playerCharacter; // 플레이어 캐릭터
    [SerializeField] private Player_ItemDrop itemCollector; // 드랍 연결

    [Header("전투 컴포넌트")]
    [SerializeField] private Player_Melee playerMelee; // 근접 공격
    [SerializeField] private Player_Shove playerShove; // 밀치기

    private bool _isFaceRight = true; // 오른쪽 보고 있는지 체크
    private bool _isDead = false; // 캐릭터 사망 체크
    private bool _isWalk = false; // 걷기 체크

    private CancellationTokenSource _healCts; // 유니테스크 취소 토큰

    public bool IsDead { get { return _isDead; } }

    private void OnEnable()
    {
        // 만약 지면 체크 오브젝트가 있으면
        if (GroundCheckObject != null)
        {
            // 지면 체크 트리거 이벤트 구독
            GroundCheckObject.GroundTriggeredEvent += OnGroundTriggered;
        }
    }

    private void OnDisable()
    {
        if (GroundCheckObject != null)
        {
            // 지면 체크 트리거 이벤트 구독 혜지
            GroundCheckObject.GroundTriggeredEvent -= OnGroundTriggered;
        }
    }

    void Start()
    {
        // 마우스 커서 자유화
        Cursor.lockState = CursorLockMode.None;

        // 마우스 커서 실체화
        Cursor.visible = true;

        // 널 체크
        if (AnimatorController == null) 
        {
            UtillLogRemove.Error("플레이어 애니메이터 연결 확인 요망!");
        }

        if (Rigidbody_Player == null)
        {
            Rigidbody_Player = GetComponent<Rigidbody2D>();

            if (Rigidbody_Player == null)
            {
                UtillLogRemove.Error("플레이어 리지드바디 연결 확인 요망!");
            }
        }

        if (GroundCheckObject == null)
        {
            UtillLogRemove.Warning("플레이어 지면체크 오브젝트 연결 확인 요망");
        }

        if (weaponManager == null)
        {
            weaponManager = GetComponent<WeaponManager>();

            if (weaponManager == null)
            {
                UtillLogRemove.Error("플레이어 웨폰 매니저(WeaponManager) 연결 확인 요망!");
            }
        }
    }

    private void Update()
    {
        // 게임매니저 싱글턴이 있고, 게임오버상태이면
        if (GameManager.Instance != null && GameManager.Instance.IsBattleActive == false)
        {
            return; 
        }

        if (_isDead) // 만약 죽었다면
        {
            return;
        }

        if (_healCts != null) // 힐킷 사용 중이라면
        {
            // 이동 속도를 0으로, 다른 키 입력을 전부 무시
            Rigidbody_Player.linearVelocity = new Vector2(0, Rigidbody_Player.linearVelocity.y);
            return;
        }

        _isWalk = Input.GetKey(KeyCode.LeftShift); // 걷기 활성화

        MoveOnUpdate(); // 움직임 함수 호출

        // 만약 스페이스바 누르고, 지면 체크가 true면
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            StartJump(); // 점프 함수 호출
        }

        if (Input.GetMouseButton(0)) // 마우스 좌 클릭 하면
        {
            // 웨폰매니저에서 발사 방향 받아오기
            bool isFired = inventoryManager.TryFireCurrentGun(!_isFaceRight);

            if (isFired == true) // 쿨타임이 0이거나 0보다 작으면
            {
                StartAttack(); // 공격 함수 호출
            }
        }

        if (Input.GetMouseButtonDown(1)) // 마우스 우클릭
        {
            // 플레이어 캐릭터가 있고, 플레이어 캐릭터에 액션함수가 있으면
            if (playerCharacter != null && playerCharacter.TryExecuteAction())
            {
                if (playerShove != null) // 플레이어가 밀치기가 있으면
                {
                    // 밀치기 실행
                    playerShove.ExecuteShove(_isFaceRight);
                }
            }
        }

        // V키 누르면
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (itemCollector != null)
            {
                // 아이템 줍기 함수 호출
                itemCollector.TryPickUp();
            }
        }

        // F키 누르면
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 플레이어 캐릭터가 있고, 플레이어 캐릭터에 액션 함수호출
            if (playerCharacter != null && playerCharacter.TryExecuteAction())
            {
                if (playerMelee != null) // 근접공격 있으면
                {
                    // 근접공격 실행
                    playerMelee.ExecuteMelee(_isFaceRight);
                }
            }
        }

        // R키 누르면
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 인베토리 매니저가 있으면
            if (inventoryManager != null)
            {
                // 인벤토리 매니저에서 재장전 함수 호출
                inventoryManager.ReloadCurrentGun();
                AnimatorController.SetState(AllState.Reload);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab)) // 탭키 누르면
        {
            BattleManager.Instance.ToggleTargetType(); // 타켓 변경
        }

        // 무기 및 아이템 스왑
        if (inventoryManager != null)
        {
            // 1번 누르면
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // 인벤토리 매니저 총 체인지 1 함수 호출
                inventoryManager.ChangeActiveGun(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2)) // 2번 누르면
            {
                // 인벤토리 매니저 총 체인지 2 함수 호출
                inventoryManager.ChangeActiveGun(2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3)) // 3번 누르면
            {
                if (inventoryManager.UseBoomItem(_isFaceRight))
                {
                    // 애니메이션 컨트롤러에서 상태 슈류탄던지기로 변경 알림
                    AnimatorController.SetState(AllState.UseGrenade);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha4)) // 4번 누르면
            {
                if (inventoryManager.HasHeelItem1())
                {
                    if (_healCts == null)
                    {
                        _healCts = new CancellationTokenSource();
                        HealRoutine(_healCts.Token).Forget();
                    }
                }
                else
                {
                    UtillLogRemove.Log("구급상자가 없습니다!");
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha5)) // 5번 누르면
            {
                int healType = inventoryManager.UseHeelItem2();

                if (healType == 1) // 진통제
                {
                    AnimatorController.SetState(AllState.UseMD); // 진통제 애니메이션 상태
                }
                else if (healType == 2) // 아드레날린
                {
                    AnimatorController.SetState(AllState.UseAD); // 아드레날린 애니메이션 상태
                }
                else
                {
                    UtillLogRemove.Log("진통제/아드레날린이 없습니다!");
                }
            }
        }
    }

    // 힐킷 사용 취소 함수
    public void CancelHealing()
    {
        // 힐킷 사용중이면
        if (_healCts  != null)
        {
            // 힐킷 사용 취소
            _healCts.Cancel();
            _healCts.Dispose();
            _healCts = null;

            UtillLogRemove.Log("힐킷 사용 취소");

            AnimatorController.SetHealing(false);
            AnimatorController.SetState(AllState.Idle);
        }
    }

    void MoveOnUpdate() // 움직임 함수
    {
        // A(-1),D(1) 입력을 받아 좌우 움직임
        float x = Input.GetAxisRaw("Horizontal");
        
        bool isBlocked = false; // 길 막힘 체크

        if (x != 0) // 만약 입력이 있으면
        {
            if (IsblodckedByEnemy(x)) // 만약 길막힘 함수 호출 되면
            {
                isBlocked = true; // 길 막힘 체크 트루
            }
        }

        if (x == 0) // 만약 좌우 입력이 없을때
        {
            // 애니매이션 대기 실행
            AnimatorController.SetState(AllState.Idle);
        }
        else // 좌우 입력 있으면
        {
            if (_isWalk == true) // 만약 걷기 상태면
            {
                AnimatorController.SetState(AllState.Walk); // 애니메이션 걷기 실행
            }
            else // 걷기 상태 아니면
            {
                AnimatorController.SetState(AllState.Run); // 애니메이션 달리기 실행
            }
        }

        if (isBlocked == true) // 만약 길이 막혔다면
        {
            // 속도는 0
            Rigidbody_Player.linearVelocity = new Vector2(0, Rigidbody_Player.linearVelocity.y);
        }
        else // 안 막혔다면
        {
            float speed; // 속도 변수 저장

            if (_isWalk == true) // 만약 걷고 있다면
            {
                // 움직임 속도의 절반
                speed = _moveSpeed / 2f;
            }
            else
            {
                // 움직임 속도 그대로
                speed = _moveSpeed;
            }

            // 속도 계산
            Rigidbody_Player.linearVelocity = new Vector2(x * speed, Rigidbody_Player.linearVelocity.y);
        }

        if (x > 0) // 만약 오른쪽으로 가고 있으면
        {
            // 만약 왼쪽을 봐라보고 있으면
            if (_isFaceRight == false)
            {
                // 뒤집기 함수 호출
                Flip();
            }
        }
        else if(x < 0) // 만약 왼쪽으로 가고 있는데
        {
            // 만약 오른쪽을 보고 있으면
            if (_isFaceRight == true)
            {
                Flip();
            }
        }
    }

    public void Die() // 사망 함수
    {
        if (_isDead == true)
        {
            return;
        }

        _isDead = true;
        // 플레이어 이동 0 못움직이게 고정
        Rigidbody_Player.linearVelocity = Vector2.zero;
        // 애니메이터 컨트롤에서 죽음로 상태 변경
        AnimatorController.SetState(AllState.Dead);

        DieRoutine().Forget(); // 사망 유니테스크 처리 함수 호출
    }

    void StartJump() // 점프 함수 
    {
        // 점프 계산
        Rigidbody_Player.linearVelocity = new Vector2(Rigidbody_Player.linearVelocity.x, _jumpForce);
        _isGrounded = false;

        // 애니메이션 컨트롤러에, 점프 애니메이션 실행 신호 보냄
        AnimatorController.SetGrounded(false);
    }

    void StartAttack() // 공격함수
    {
        // 공격 애니메이션 호출
        AnimatorController.SetState(AllState.Attack);
    }

    private void OnGroundTriggered(bool isGrounded) // 지면 체크 센서 트리거 함수
    {
        // 지면 체크 결과 저장
        _isGrounded = isGrounded;

        if (_isGrounded == true) // 땅 체크가 트루면
        {
            // 애니메이션컨트롤러에 지상도착 알림
            AnimatorController.SetGrounded(true);
            // 대기 애니메이션 실행
            AnimatorController.SetState(AllState.Idle);
        }
    }

    private void Flip() // 캐릭터 반전 함수
    {
        _isFaceRight = !_isFaceRight; // 현재 캐릭터 보는 방향  뒤집기
        Vector3 scaler = transform.localScale; // 캐릭터 정보 저장
        scaler.x *= -1; // 실제로 캐릭터 보는 방향 뒤집기
        transform.localScale = scaler; // 뒤집은 캐릭터 정보 저장
    }
    
    // 적이 길을 막고 있는지 체크 함수
    private bool IsblodckedByEnemy(float direction)
    {
        // 거리 저장
        float checkDistance = 0.5f;
        // 갈 방향 저장
        Vector2 raycastDirection = new Vector2(direction, 0);
        // 플레이어로 부터 저장한 거리까지 레이캐스트를 쏘고, 레이캐스트에 적이 맞으면 저장
        RaycastHit2D hit = Physics2D.Raycast(transform.position, raycastDirection, checkDistance, LayerMask.GetMask("Enemy"));
        
        if (hit.collider != null) // 저장된 콜라이더가 있으면
        {
            return true; // true 반환
        }
        else // 아니면
        {
            return false;// false 반환
        }
    }

    // 사망 유니테스크 처리 함수
    private async UniTaskVoid DieRoutine() 
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
    }

    // 힐 킷 사용 유니테스크 처리 함수
    private async UniTaskVoid HealRoutine(CancellationToken token)
    {
        // 애니메이션 컨트롤러에서 상태를 힐 킷사용으로 변환 알림
        AnimatorController.SetState(AllState.UseHeal);
        // 애니메이션 컨트롤러에서 힐킷사용중을 트루로 변환 알림
        AnimatorController.SetHealing(true);

        try
        {
            // 힐 킷 사용 딜레이 3초, 힐 킷 사용 캔슬되면 바로 취소
            await UniTask.Delay(TimeSpan.FromSeconds(3.0f), cancellationToken: token);

            // 인벤토리 매니저가 있으면
            if (inventoryManager != null)
            {
                // 인벤토리매니저에서 힐탬사용2 함수 호출
                inventoryManager.UseHeelItem1();
            }

            UtillLogRemove.Log("힐 킷 사용 회복 완료");
        }
        // 취소되면
        catch (OperationCanceledException)
        {
            UtillLogRemove.Log("힐 킷 사용이 취소되었습니다.");
        }
        finally // 끝나면
        {
            // 애니메이션 컨트롤러 힐킷 사용중 거짓으로 변경
            AnimatorController.SetHealing(false);
            AnimatorController.SetState(AllState.Idle);

            if (_healCts != null)
            {
                // 취소 버튼 폐기
                _healCts.Dispose();
                // 힐 킷 사용 널
                _healCts = null;
            }

        }
    }

    // 캐릭터 삭제시 함수
    private void OnDestroy()
    {
        // 힐 킷 사용 중이면
        if (_healCts != null)
        {
            // 힐 킷 사용 중지
            _healCts.Cancel();
            // 힐 킷 파괴
            _healCts.Dispose();
        }
    }
}
