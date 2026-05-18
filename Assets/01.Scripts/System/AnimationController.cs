using UnityEngine;
using System;
using System.Collections.Generic;

public enum AllState // 플레이어 상태
{
    Idle,
    Walk,
    Run,
    Attack,
    Dead
}

// 애니매이션 전체 조작을 담당하는 컨트롤러
public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator Animator_Control; // 애니메이터 연결

    private AllState _currentState; // 현재 상태

    private Dictionary<AllState, Action> _animationActions; // 딕셔너리로 모든 애니메이션 행동 저장

    // 애니매이션 상태를 해시 숫자로 저장(한번 선언 시 변환 불가)
    private static readonly int AnimationIsRun = Animator.StringToHash("IsRun");
    private static readonly int AnimationIsDead = Animator.StringToHash("IsDead");
    private static readonly int AnimationIsAttack = Animator.StringToHash("IsAttack");
    private static readonly int AnimationIsWalk = Animator.StringToHash("IsWalk");
    private static readonly int AnimationIsGrounded = Animator.StringToHash("isGrounded");

    private void Awake()
    {
        // 상태에 맞는 행동을 딕셔너리로 저장
        _animationActions = new Dictionary<AllState, Action>();

        // 각 상태에 따른 애니메이션 실행 함수 연결
        _animationActions.Add(AllState.Idle, AllIdleAnimation);
        _animationActions.Add(AllState.Walk, AllWalkAnimation);
        _animationActions.Add(AllState.Run, AllRunAnimation);
        _animationActions.Add(AllState.Dead, AllDeadAnimation);
        _animationActions.Add(AllState.Attack, AllAttackAnimation);
    }

    private void Start()
    {
        // 시작할때 만약 연결된 애니메이션이 없다면
        if (Animator_Control == null)
        {
            // 애니메이션 컴포넌트에서 애니메이션을 가져오고
            Animator_Control = GetComponent<Animator>();

            // 만약 애니메이션을 못찾으면
            if (Animator_Control == null)
            {
                // 디버그 로그 띄우기
                UtillLogRemove.Error("애니메이터가 연결되지 않았습니다! 확인해주세요.");
            }
        }
    }

    // 외부(플레이어, 적)에서 상태를 바꿀때 함수를 호출하는 함수
    public void SetState(AllState newState)
    {
        // 만약 바뀌는 상태와 현재상태가 같으면
        if (newState == _currentState)
        {
            // 만약  현재상태가 공격이 아니면
            if (newState != AllState.Attack)
            {
                return; // 반환
            }
        }

        // 만약 현재상태가 딕셔너리에 있다면
        if (_animationActions.TryGetValue(newState, out Action action))
        {
            // 액션 실행
            action.Invoke();
            // 현재상태를 다음상태로 저장
            _currentState = newState;
        }
        else
        {
            // 딕셔너리에 저장안된 함수 호출시 로그 호출
            UtillLogRemove.Warning($"{newState} 연결된 애니메이션이 없습니다.");
        }
    }

    public void SetGrounded(bool isGrounded) // 점프 애니메이션 함수
    {
        if (Animator_Control != null)
        {
            SafeSetBool(AnimationIsGrounded, isGrounded);
        }
    }

    private void AllIdleAnimation() // 대기애니메이션 함수
    {
        // 상태 초기화 함수 호출
        ResetAllBoolParameters();
    }

    private void AllWalkAnimation() // 걷는 애니매이션 함수
    {
        ResetAllBoolParameters();
        // 걷기 애니메이션 실행
        SafeSetBool(AnimationIsWalk, true);
    }

    private void AllRunAnimation() // 달리기 애니메이션 함수
    {
        ResetAllBoolParameters();
        // 달리기 애니메이션 실행
        SafeSetBool(AnimationIsRun, true);
    }

    private void AllDeadAnimation() // 죽음 애니메이션 함수
    {
        ResetAllBoolParameters();
        // 죽음 애니메이션 실행
        SafeSetBool(AnimationIsDead, true);
    }

    private void AllAttackAnimation() // 공격 애니메이션 함수
    {
        ResetAllBoolParameters();
        // 공격 애니메이션 실행
        SafeSetBool(AnimationIsAttack, true);
    }

    // 상태 초기화 함수
    private void ResetAllBoolParameters()
    {
        // 걷기 애니메이션 끄기
        SafeSetBool(AnimationIsWalk, false);
        // 달리기 애니메이션 끄기
        SafeSetBool(AnimationIsRun, false);
        // 죽음 애니메이션 끄기
        SafeSetBool(AnimationIsDead, false);
        // 공격 애니메이션 끄기
        SafeSetBool(AnimationIsAttack, false);
    }

    // 애니메이션 파라미터 조정 함수
    private void SafeSetBool(int parameterHash, bool value)
    {
        if (Animator_Control == null) // 애니메이션 컨트롤러가 없으면
        {
            return; // 반환
        }

        // 애니메이터가 가진 파라미터 목록을 하니씩 검사
        foreach (AnimatorControllerParameter param in Animator_Control.parameters)
        {
            // 해시 번호 바꾸려는 번호와 맞는지 체크
            if (param.nameHash == parameterHash)
            {
                // 스위치가 실제로 있을때만 값 변경
                Animator_Control.SetBool(parameterHash, value);
                return; // 반환
            }
        }
    }
}
