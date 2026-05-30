using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public enum AllState // 공유할 상태 목록
{
    Idle,
    Walk,
    Run,
    Attack,
    Dead,
    Hit,
    Jump,
    UseHeal, // 힐킷
    UseInstantHeal, // 구급얍, 아드레날린
    UseGrenade // 슈륙탄
}

// 애니매이션 전체 조작을 담당하는 컨트롤러
public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator Animator_Control; // 애니메이터 연결

    private AllState _currentState; // 현재 상태 저장

    // 애니매이션 상태를 해시 숫자로 저장(한번 선언 시 변환 불가)
    // bool 스위치 (재생이 유지 되야하는 경우)
    private static readonly int AnimationIsWalk = Animator.StringToHash("IsWalk");
    private static readonly int AnimationIsRun = Animator.StringToHash("IsRun");
    private static readonly int AnimationIsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int AnimationIsDead = Animator.StringToHash("IsDead");
    private static readonly int AnimationIsHealing = Animator.StringToHash("IsHealing");

    // Trigger 스위치 (한번씩만 재생하는 경우)
    private static readonly int AnimationIsAttack = Animator.StringToHash("IsAttack");
    private static readonly int AnimationTriggerHit = Animator.StringToHash("IsHit");
    private static readonly int AnimationTriggerGrenade = Animator.StringToHash("Grenade");
    private static readonly int AnimationTriggerInstantHeal = Animator.StringToHash("InstantHeal");

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
            // 만약  현재상태가 공격, 피격, 슈륙탄 투척, 힐킷사용이 아니면( 연속 재생 가능 경우 애니메이션)
            if (newState != AllState.Attack && newState != AllState.Hit && newState != AllState.UseGrenade && newState != AllState.UseInstantHeal) 
            {
                return; // 반환
            }
        }

        // 기존 스위치들 모두 끄기
        ResetAllBoolParameters();

        switch (newState)
        {
            case AllState.Idle:
                break;
            case AllState.Walk:
                SafeSetBool(AnimationIsWalk, true);
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
            case AllState.UseHeal:
                SafeSetBool(AnimationIsHealing, true);
                break;
            case AllState.UseInstantHeal:
                SafeSetTrigger(AnimationTriggerInstantHeal);
                break;
            case AllState.UseGrenade:
                SafeSetTrigger(AnimationTriggerGrenade);
                break;
            default:
                UtillLogRemove.Warning($"{newState} 상태에 대한 처리가 switch문에 없습니다.");
                break;
        }

        _currentState = newState; // 바뀐 상태 저장
    }

    // 점프 애니메이션 함수 (점프 중에도 다른 동작으로 변환 할 수 있도록 따로 호출)
    public void SetGrounded(bool isGrounded)
    {
        if (Animator_Control != null)
        {
            // isGrounded를 스스로 끄거나 킴
            SafeSetBool(AnimationIsGrounded, isGrounded);
        }
    }

    // 힐킷 애니메이션 함수 (힐킷 사용중에도 다른 동작으로 변환 할 수 있도록 따로 호출
    public void SetHealing(bool isHealing) 
    {
        if (Animator_Control != null)
        {
            SafeSetBool(AnimationIsHealing, isHealing);
        }
    }

    // 상태 초기화 함수
    private void ResetAllBoolParameters()
    {
        // 걷기 애니메이션 끄기
        SafeSetBool(AnimationIsWalk, false);
        // 달리기 애니메이션 끄기
        SafeSetBool(AnimationIsRun, false);
        // 힐킷 애니메이션 끄기
        SafeSetBool(AnimationIsHealing, false);
    }

    // 애니메이션 스위치 조작 함수
    private void SafeSetBool(int parameterHash, bool value)
    {
        if (Animator_Control == null) // 애니메이션 컨트롤러가 없으면
        {
            return; // 반환
        }

        // 애니메이터가 가진 파라미터 목록을 하니씩 검사
        foreach (AnimatorControllerParameter animatorParam in Animator_Control.parameters)
        {
            // 해시 번호로 파라미터가 있는지 확인
            if (animatorParam.nameHash == parameterHash)
            {
                // 스위치가 실제로 있을때만 값 변경
                Animator_Control.SetBool(parameterHash, value);

                return; // 반환
            }
        }
    }

    // 애니메이션 트리거 조작 함수
    private void SafeSetTrigger(int parameterHash)
    {
        if (Animator_Control == null) // 널 체크
        {
            return;
        } 

        foreach (AnimatorControllerParameter animatorParam in Animator_Control.parameters)
        {
            if (animatorParam.nameHash == parameterHash)
            {
                // 트리거 발동
                Animator_Control.SetTrigger(parameterHash);

                return;
            }
        }
    }
}
