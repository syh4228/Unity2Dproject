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

    [Header("스테미너")]
    public int MaxStamina; // 최대 스테미나
    public int currentStamina; // 현재 스테미나

    [Header("경직 설정")]
    [SerializeField] private float stunTime = 0.5f;// 경직시간

    [Header("컴포넌트 연결")]
    [SerializeField] private Player_Controller playerController; // 실제 사망처리할 플레이어 컨트롤러 연결
    [SerializeField] private AnimationController animatorController; // 애니메이션 컨트롤러 연결

    private bool _isDead = false; // 죽음 체크
    public bool isStunned = false; // 피격 체크

    public event Action<int, int> OnHpChanged; // UI에 최대체력과, 현재 체력 알려주기

    private void Start()
    {
       currentHp = MaxHp; // 시작시 최대체력은 = 현재체력

        if (playerController == null)
        {
            playerController = GetComponent<Player_Controller>();
        }

        if (UIManager.Instance != null) // UI매니저 있으면
        {
            // UI매니저 함수 호출
            if (UIManager.Instance.GetBattleUI() != null)
            {
                // 현재 체력, 임시체력(소수점 버리기), 최대체력 가져오기
                UIManager.Instance.GetBattleUI().UpdateHealthUI(currentHp, Mathf.CeilToInt(currentTempHp), MaxHp);
            }
        }
    }

    // 임시 체력을 실시간으로 깍아주기
    private void Update()
    {
        if (_isDead ==  true)
        {
            return;
        }

        if(currentTempHp > 0f) // 임시체력이 0보다 크면
        {
            // 현재 임시체력은 시간 * 깍는 값 빼고 남은 값
            currentTempHp -= tempHpDecayRate * Time.deltaTime;

            if (currentTempHp < 0f) // 만약 0보다 작으면
            {
                currentTempHp = 0f; // 임시체력은 0
            }

            if(UIManager.Instance != null) // UI매니저 있고
            {
                if (UIManager.Instance.GetBattleUI() != null) // 배틀UI 있으면
                {
                    // 현재 체력, 임시체력(소수점 버리기), 최대체력 가져오기
                    UIManager.Instance.GetBattleUI().UpdateHealthUI(currentHp, Mathf.CeilToInt(currentTempHp), MaxHp);
                }
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

        if (UIManager.Instance != null) // 만약 UI 매니저 있으면
        {
            if (UIManager.Instance.GetBattleUI() != null)
            {
                // 현재 체력, 임시체력(소수점 버리기), 최대체력 가져오기
                UIManager.Instance.GetBattleUI().UpdateHealthUI(currentHp, Mathf.CeilToInt(currentTempHp), MaxHp);
            }
        }

        if (currentHp <= 0) // 만약 체력이 0이하면
        {
            currentHp = 0; // 현재 체력 0
            Die(); // 사망함수 호출
        }
        else
        {
            // 피격 시 스턴 함수 호출
            StartCoroutine(HitStunCharacter());
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
        await UniTask.Delay(1000);

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
        if (currentHp + currentTempHp >  MaxHp)
        {
            // 임시체력은 최대체력 - 현재체력 (최종값이 최대체력 만들기)
            currentTempHp = MaxHp - currentHp;
        }

        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.GetBattleUI() != null)
            {
                UIManager.Instance.GetBattleUI().UpdateHealthUI(currentHp, Mathf.CeilToInt(currentTempHp), MaxHp);
            }
        }
    }

    // 피격시 경직 함수
    private IEnumerator HitStunCharacter()
    {
        isStunned = true;

        if (animatorController != null)
        {
            animatorController.AllHitAnimation();
        }

        yield return new WaitForSeconds(stunTime);

        isStunned = false;

        // 애니메이션 대기로 전환
        animatorController.SetState(AllState.Idle);
    }
}
