using UnityEngine;

// 적 애니메이션 공통 파라미터 해시 변환용 컴포넌트
public class Enemy_AnimationHashes : MonoBehaviour
{
    public static readonly int Idle = Animator.StringToHash("IsIdle");
    public static readonly int Hit = Animator.StringToHash("IsHit");
    public static readonly int Run = Animator.StringToHash("IsRun");
    public static readonly int Attack = Animator.StringToHash("IsAttack");
    public static readonly int Die = Animator.StringToHash("IsDead");
}
