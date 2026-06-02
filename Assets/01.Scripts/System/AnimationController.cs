using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

// 애니매이션 전체 조작을 담당하는 컨트롤러
public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator Animator_Control; // 애니메이터 연결

    private AllState _currentState; // 현재 상태 저장

    // 애니메이션 클립을 동적으로 갈아끼우기 위한 오버라이드 컨트롤러 저장
    private AnimatorOverrideController _overrideController; 

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
    private static readonly int AnimationTriggerMelee = Animator.StringToHash("Melee");
    private static readonly int AnimationTriggerShove = Animator.StringToHash("Shove");
    private static readonly int AnimationTriggerDrop = Animator.StringToHash("Drop");
    private static readonly int AnimationTriggerReload = Animator.StringToHash("IsReload");
    private static readonly int AnimationTriggerUseMD = Animator.StringToHash("UseMD");
    private static readonly int AnimationTriggerUseAD = Animator.StringToHash("UseAD"); 

    // 공격 속도
    private static readonly int AnimationAttackSpeed = Animator.StringToHash("AttackSpeed");
    
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

        if (Animator_Control != null) // 애니메이터 컨트롤러가 있으면
        {
            // 오버라이브 컨트롤러 저장
            _overrideController = new AnimatorOverrideController(Animator_Control.runtimeAnimatorController);
            // 원본 애니메이터를 오버라이브 컨트롤러에 저장
            Animator_Control.runtimeAnimatorController = _overrideController;
        }
    }

    // 외부(플레이어, 적)에서 상태를 바꿀때 함수를 호출하는 함수
    public void SetState(AllState newState)
    {
        // 만약 바뀌는 상태와 현재상태가 같으면
        if (newState == _currentState)
        {
            // 만약 현재 상태가 ~ 아니면
            if (newState != AllState.Attack && // 공격
                newState != AllState.Hit && // 피격
                newState != AllState.UseGrenade && // 슈륙탄 투척
                newState != AllState.UseMD &&    // 진통제 
                newState != AllState.UseAD && // 아드
                newState != AllState.Melee && // 근접 공격
                newState != AllState.Shove && // 밀치기
                newState != AllState.Drop) // 줍기
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
            case AllState.UseMD:
                SafeSetTrigger(AnimationTriggerUseMD);
                break;
            case AllState.UseAD:
                SafeSetTrigger(AnimationTriggerUseAD);
                break;
            case AllState.UseGrenade:
                SafeSetTrigger(AnimationTriggerGrenade);
                break;
            case AllState.Melee:
                SafeSetTrigger(AnimationTriggerMelee);
                break;
            case AllState.Shove:
                SafeSetTrigger(AnimationTriggerShove);
                break;
            case AllState.Drop:
                SafeSetTrigger(AnimationTriggerDrop);
                break;
            case AllState.Reload:
                SafeSetTrigger(AnimationTriggerReload);
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

    // 공격 속도 함수
    public void SetAttackSpeed(float rpm)
    {
        if (Animator_Control == null) return; // 애니메이터 컨트롤 없으면 반환

        // RPM 속도 저장
        float speedRatio = rpm / 60f;

        // 애니메이션 속도 RPM에 맞춰 변경
        Animator_Control.SetFloat(AnimationAttackSpeed, speedRatio);
    }

    // 무기에 따른 애니메이션 교체 함수
    public async UniTaskVoid ChangeWeaponAnimation(string attackClipPath, string reloadClipPath)
    {
        // 오버라이드컨트롤이 없거나, 경로에 애니메이션 클립이 없으면 반환
        if (_overrideController == null) return;

        if (!string.IsNullOrEmpty(attackClipPath))
        {
            AnimationClip newAttackClip = null;

            try
            {
                // 어드레서블에서 찾기
                newAttackClip = await ResourceManager.Inst.LoadAsset<AnimationClip>(attackClipPath);
            }
            catch { }

            if (newAttackClip == null)
            {
                newAttackClip = Resources.Load<AnimationClip>(attackClipPath);
            }

            if (newAttackClip != null) // 만약 있으면
            {
                // 기본권총 사격을 공격 애니메이션을 오버라이드 컨트롤에 저장
                _overrideController["Prey_RT"] = newAttackClip;

                UtillLogRemove.Log($"애니메이션 교체 완료: {attackClipPath}");
            }
            else
            {
                UtillLogRemove.Warning($"애니메이션 클립을 찾을 수 없습니다: Resources/{attackClipPath}");
            }
        }

        if (!string.IsNullOrEmpty(reloadClipPath))
        {
            AnimationClip newReloadClip = null;

            try
            {
                // 어드레서블에서 찾기
                newReloadClip = await ResourceManager.Inst.LoadAsset<AnimationClip>(reloadClipPath);
            }
            catch { }

            // 어드레서블에 없으면 기존 Resources 폴더에서 찾기
            if (newReloadClip == null)
            {
                newReloadClip = Resources.Load<AnimationClip>(reloadClipPath);
            }

            if (newReloadClip != null)
            {
                _overrideController["Prey_RT_Reload"] = newReloadClip;

                UtillLogRemove.Log($"애니메이션 교체 완료: {newReloadClip}");
            }
            else
            {
                UtillLogRemove.Warning($"애니메이션 클립을 찾을 수 없습니다: Resources/{newReloadClip}");
            }

        }
    }
}
