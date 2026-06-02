using UnityEngine;

public class Enemy_AnimationController : MonoBehaviour
{
    [SerializeField] private Animator Animator_Control; // 애니메이터 컨트롤 연결

    private AllState _currentState; // 현재 상태 저장

    // 불 타입 해시 파라미터
    private static readonly int AnimationIsRun = Animator.StringToHash("IsRun");
    private static readonly int AnimationIsDead = Animator.StringToHash("IsDead");

    // 트리거 해시 파라미터
    private static readonly int AnimationIsAttack = Animator.StringToHash("IsAttack");
    private static readonly int AnimationTriggerHit = Animator.StringToHash("IsHit");
    private static readonly int AnimationIsJumpAttack = Animator.StringToHash("IsJumpAttack");

    private void Start()
    {
        // 애니메이터 컨트롤이 없으면
        if (Animator_Control == null)
        {
            // 애니메이터 컴포넌트 찾아서 가져오기
            Animator_Control = GetComponent<Animator>();

            // 컴포넌트 없으면
            if (Animator_Control == null)
            {
                UtillLogRemove.Error("적 애니메이터가 연결되지 않았습니다.");
            }
        }
    }

    // 상태 변화 함수
    public void SetState(AllState newState)
    {
        // 현재 상태와, 새로운 상태가 같으면
        if (newState == _currentState)
        {
            // 새로운 상태가 공격이나, 피격이 아니면
            if (newState != AllState.Attack && newState != AllState.Hit)
            {
                return; // 반환
            }
        }

        // 기존 상태들 초기화 함수 호출
        ResetAllBoolParameters();

        switch (newState)
        {
            case AllState.Idle: // 대기는 기본 상태
                break;
            case AllState.Run:
                SafeSetBool(AnimationIsRun, true);
                break;
            case AllState.Dead:
                SafeSetBool(AnimationIsDead, true);
                break;
            case AllState.Attack:
                SafeSetTrigger(AnimationIsAttack);
                break;
            case AllState.Hit:
                SafeSetTrigger(AnimationTriggerHit);
                break;
            case AllState.JumpAttack:
                SafeSetTrigger(AnimationIsJumpAttack);
                break;
            default:
                break;
        }

        _currentState = newState; // 바뀐 상태를 현재 상태로 저장
    }

    // 상태 초기화 함수
    private void ResetAllBoolParameters()
    {
        // 달리기 상태 거짓으로 변환
        SafeSetBool(AnimationIsRun, false);
    }

    // 애니메이션 스위치 조작 함수
    private void SafeSetBool(int parameterHash, bool value)
    {
        if (Animator_Control == null) return; // 애니메이터 컨트롤 없으면 반환

        // 하나씩 파라미터 꺼내서 저장
        foreach (AnimatorControllerParameter param in Animator_Control.parameters)
        {
            // 저장한 파라미터와 해시번호가 같으면
            if (param.nameHash == parameterHash)
            {
                // 스위치 값 변경
                Animator_Control.SetBool(parameterHash, value);
                return; // 반환
            }
        }
    }

    // 애니메이션 트리거 조작 함수
    private void SafeSetTrigger(int parameterHash)
    {
        if (Animator_Control == null) return;

        foreach (AnimatorControllerParameter param in Animator_Control.parameters)
        {
            if (param.nameHash == parameterHash)
            {
                Animator_Control.SetTrigger(parameterHash);
                return;
            }
        }
    }
}
