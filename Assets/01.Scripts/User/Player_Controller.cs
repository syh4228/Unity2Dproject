using System.Collections;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [Header("플레이어 설정")]
    [SerializeField] private float _moveSpeed = 2f; // 이동속도(기본 달리기)

    [Header("공격 설정")]
    [SerializeField] private float _attackCooldown = 0.5f; // 공격 쿨타임
    private float _cooldownTimer = 0f; // 쿨타임 계산

    [Header("점프 및 물리")]
    [SerializeField] private float _jumpForce = 7f; // 점프 힘
    [SerializeField] private bool _isGrounded; // 지면에 닿았는지 확인

    [Header("컴포넌트")]
    [SerializeField] private Rigidbody2D Rigidbody_Player; // 리지드바디
    [SerializeField] private GroundCheck GroundCheckObject; // 지면체크 스크립트를 연결한 자식 오브젝트
    [SerializeField] private AnimationController AnimatorController; // 애니메이션 
    [SerializeField] private WeaponManager weaponManager; // 웨폰 매니저 연결
    [SerializeField] private Player_InventoryManager inventoryManager; // 인벤토리 매니저

    private bool _isFaceRight = true; // 오른쪽 보고 있는지 체크
    private bool _isDead = false; // 캐릭터 사망 체크
    private bool _isWalk = false; // 걷기 체크

    public bool IsDead
    {
        get { return _isDead; }
    }

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

        if (_cooldownTimer > 0f) // 만약 쿨타임이 0 보다 크면
        {
            // 쿨타임 시작
            _cooldownTimer = _cooldownTimer - Time.deltaTime;
        }

        // 만약 쉬프트 누르면
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _isWalk = true; // 트루 처리
        }
        else // 안누르면
        {
            _isWalk = false; // 거짓 처리
        }

        MoveOnUpdate(); // 움직임 함수 호출

        // 만약 스페이스바 누르면
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if ( _isGrounded) // 만약 지면에 붙어 있으면
            {
                StartJump(); // 점프 함수 호출
            }    
        }

        if (Input.GetMouseButtonDown(0)) // 마우스 클릭하면
        {
            if (_cooldownTimer <= 0f) // 쿨타임이 0이거나 0보다 작으면
            {
                StartAttack(); // 공격 함수 호출
            }
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
        Rigidbody_Player.linearVelocity = Vector2.zero;
        AnimatorController.SetState(AllState.Dead);

        StartCoroutine(DieRoutine()); // 사망 코루틴 함수 호출
    }

    void StartJump() // 점프 함수 
    {
        // 점프
        Rigidbody_Player.linearVelocity = new Vector2(Rigidbody_Player.linearVelocity.x, _jumpForce);
        _isGrounded = false;

        // 애니메이션 컨트롤러에, 점프 애니메이션 실행 신호 보냄
        AnimatorController.SetGrounded(false);
    }

    void StartAttack() // 공격함수
    {
        // 공격 애니메이션 호출
        AnimatorController.SetState(AllState.Attack);

        if (inventoryManager != null) // 인벤토리 매니저가 있으면
        {
            // 현재 총 발사 함수 호출
            inventoryManager.FireCurrentGun(!_isFaceRight);
        }

        _cooldownTimer = _attackCooldown; // 쿨타임 초기화
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

    private IEnumerator DieRoutine() // 사망 코투틴 함수
    {
        yield return new WaitForSeconds(0.3f);
    }
}
