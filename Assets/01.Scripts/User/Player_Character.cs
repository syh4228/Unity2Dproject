using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

public class Player_Character : MonoBehaviour
{
    [Header("체력 설정")]
    public int MaxHp = 100;// 최대 체력
    public int currentHp; // 현재 체력

    [Header("임시 체력 설정")]
    public float currentTempHp = 0f; // 현재 임시체력
    [SerializeField] public float tempHpDecayRate = 2f; // 임시체력 지속력

    [Header("스테미너 설정")]
    public int MaxStamina = 100; // 최대 스테미나
    public int currentStamina; // 현재 스테미나
    [SerializeField] private int staminaRecoveryRate = 10; // 초당 회복량
    [SerializeField] private float lowStaminaThreshold = 30f; // 스테미너 패널티 기준
    [SerializeField] private float lowStaminaPenaltyMultiplier = 0.2f; // 패널티 상태시 스테미너의 소모량
    [SerializeField] private float actionCooldown = 1.5f; // 패널티 상태시 특수 공격 쿨타임

    [Header("경직 설정")]
    [SerializeField] private float stunTime = 0.5f;// 경직시간

    [Header("컴포넌트 연결")]
    [SerializeField] private Player_Controller playerController; // 실제 사망처리할 플레이어 컨트롤러 연결
    [SerializeField] private AnimationController animatorController; // 애니메이션 컨트롤러 연결

    private bool _isDead = false; // 죽음 체크
    public bool isStunned = false; // 피격 체크

    private float lastActionTime = 0f; // 특수 공격 쿨타임

    public event Action<int, int> OnHpChanged; // UI에 최대체력과, 현재 체력 알려주기
    public bool isAdrenalineActive = false; // 아드레날린 버프 상태 관리

    private void Start()
    {
        currentHp = MaxHp; // 시작시 최대체력은 = 현재체력
        currentStamina = MaxStamina; // 쵀대 스테미나는 = 현재 스테미나

        if (playerController == null)
        {
            playerController = GetComponent<Player_Controller>();
        }

        // UI 갱신 함수 호출
        UpdateHealthUI_Internal();
    }

    // 임시 체력을 실시간으로 깍아주기
    private void Update()
    {
        if (_isDead == true)
        {
            return;
        }

        if (currentTempHp > 0f) // 임시체력이 0보다 크면
        {
            // 현재 임시체력은 시간 * 깍는 값 빼고 남은 값
            currentTempHp -= tempHpDecayRate * Time.deltaTime;

            if (currentTempHp < 0f) // 만약 0보다 작으면
            {
                currentTempHp = 0f; // 임시체력은 0
            }

            // UI 갱신 함수 호출
            UpdateHealthUI_Internal();
        }

        // 스테미너 자동 회복
        if (currentStamina < MaxStamina) // 최대스테미너보다 현재 스테미너가 낮으면
        {
            // 스테미너 회복
            currentStamina += Mathf.RoundToInt(staminaRecoveryRate * Time.deltaTime);
            // 만약 현재 스테미너가 최대보다 많으면 최대스테미너와 같게 변경
            if (currentStamina > MaxStamina)
            {
                currentStamina = MaxStamina;
            }
        }
    }

    public void TakeDamage(int damage) // 외부에서 대미지 받아오는 함수
    {
        if (_isDead == true) // 만약 죽었으면 
        {
            return; // 반환
        }

        currentHp -= damage; // 대미지 만큼 체력 감소
        UtillLogRemove.Log($"플레이어 피격, 남은 체력:{currentHp}");

        UpdateHealthUI_Internal(); // UI 갱신

        if (currentHp <= 0) // 만약 체력이 0이하면
        {
            currentHp = 0; // 현재 체력 0
            Die(); // 사망함수 호출
        }
        else
        {
            // 피격 시 스턴 함수 호출
            HitStunCharacter().Forget();
        }

        if (OnHpChanged != null) // OnHpChanged가 있으면
        {
            OnHpChanged(currentHp, MaxHp);// 현재최력, 최대체력 알리기
        }
    }

    private async void Die() // 사망 함수
    {
        if (_isDead) // 죽어있다면
        {
            return; // 반환
        }

        _isDead = true;

        // 플레이어 컨트롤러가 있으면
        if (playerController != null)
        {
            // 플레이어 컨트롤러 다이 함수 호출
            playerController.Die();
        }

        gameObject.layer = LayerMask.NameToLayer("PlayerDead");

        // 죽고 나서 대기
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: this.GetCancellationTokenOnDestroy());

        // 게임매니저가 있으면
        if (GameManager.Instance != null)
        {
            // 플레이어 사망 이벤트 호출
            GameManager.Instance.BtnClick_PlayerDie();
        }
    }

    // 임시체력 추가 적용 함수
    public void AddTemporaryHealth(float amount)
    {
        if (_isDead == true)
        {
            return;
        }

        currentTempHp += amount; // 임시체력은 임시회복량 + 임시체력 값

        //  현재 체력 + 임시 현재 체력 이 최대체력보다 많으면
        if (currentHp + currentTempHp > MaxHp)
        {
            // 임시체력은 최대체력 - 현재체력 (최종값이 최대체력 만들기)
            currentTempHp = MaxHp - currentHp;
        }

        UpdateHealthUI_Internal(); // UI 갱신
    }

    // 피격시 경직 함수
    private async UniTaskVoid HitStunCharacter()
    {
        isStunned = true; // 스턴 트루

        if (animatorController != null) // 애니메이션 컨트롤 있으면
        {
            // 애니메이션 컨트롤러에서 상태 힛으로 변경
            animatorController.SetState(AllState.Hit);
        }

        // 유니테스크 대기 (오브젝트 파괴 시 자동 취소되는 안전장치 토큰 추가)
        await UniTask.Delay(TimeSpan.FromSeconds(stunTime), cancellationToken: this.GetCancellationTokenOnDestroy());

        isStunned = false; // 스턴 거짓

        if (animatorController != null) // 애니메이션 컨트롤 있으면
        {
            // 애니메이션 컨트롤에서 상태 대기로 변경
            animatorController.SetState(AllState.Idle);
        }
    }

    // 구급킷 사용 함수
    public void ApplyHeal_Hk()
    {
        if (_isDead) return;

        // 잃은 체력 저장
        int lostHp = MaxHp - currentHp;
        // 잃은 체력 80프로 회복
        int heelAmount = Mathf.RoundToInt(lostHp * 0.8f);

        currentHp += heelAmount; // 현재최력에 회복할 체력 더하기

        if (currentHp < 90) // 현재 체력 90 이 안되면
        {
            currentHp = 90; // 90으로 조정
        }

        if (currentHp > MaxHp) // 현재최력이 최대 체력보다 크면
        {
            currentHp = MaxHp; // 현재최력 최대체력으로
        }

        currentTempHp = 0f; // 임시체력 0으로

        UpdateHealthUI_Internal();
    }

    // 구급약 사용 함수
    public void ApplyHeal_MD()
    {
        if (_isDead) return;

        // 임시 체력 50 회복
        AddTemporaryHealth(50f);
    }

    // 아드레날린 상용 함수
    public void ApplyHeal_AD()
    {
        if (_isDead) return;

        // 임시 체력 30 회복
        AddTemporaryHealth(30f);

        // 아드레날린 버프 켜기
        isAdrenalineActive = true;
        UtillLogRemove.Log("아드레날린 발동! 스태미너 소모 0, 이동속도 증가!");

        // 아드레날린 버프 끄기 함수 호출
        TurnOffAdrenalineRoutine().Forget();
    }

    // 아드렌 날린 버프 끄기 함수
    private async UniTaskVoid TurnOffAdrenalineRoutine()
    {
        // 10초 대기 (플레이어가 파괴되면 자동으로 취소됨)
        await UniTask.Delay(TimeSpan.FromSeconds(10f), cancellationToken: this.GetCancellationTokenOnDestroy());

        isAdrenalineActive = false;
        UtillLogRemove.Log("아드레날린 효과 종료.");
    }

    // UI 갱신 함수
    private void UpdateHealthUI_Internal()
    {
        if (UIManager.Instance != null && UIManager.Instance.GetBattleUI() != null)
        {
            UIManager.Instance.GetBattleUI().UpdateHealthUI(currentHp, Mathf.CeilToInt(currentTempHp), MaxHp);
        }
    }

    // 밀치기,근접공격 사용 함수 (특수 공격 가능하면 true, 불가능하면 false)
    public bool TryExecuteAction()
    {
        // 아드레날린 버프 상태면 스테미너 소모 없음
        if (isAdrenalineActive) return true;

        // 쿨타임 체크 (스테미너가 낮을 때만 적용)
        if (currentStamina < lowStaminaThreshold && Time.time - lastActionTime < actionCooldown)
        {
            UtillLogRemove.Log("스테미너 부족으로 쿨타임 중!");
            return false;
        }

        // 스테미너 소모
        int staminaCost;
        if (currentStamina < lowStaminaThreshold)
        {
            // 30 미만일 때: 남은 스테미너의 20% 소모
            staminaCost = Mathf.CeilToInt(currentStamina * lowStaminaPenaltyMultiplier);
            lastActionTime = Time.time; // 쿨타임 시작
        }
        else
        {
            // 30 이상일 때: 기본 20 소모
            staminaCost = 20;
        }

        // 스테미너가 충분한지 최종 확인
        if (currentStamina >= staminaCost)
        {
            currentStamina -= staminaCost;
            return true;
        }
        else
        {
            UtillLogRemove.Log("스테미너 부족!");
            return false;
        }
    }
}
