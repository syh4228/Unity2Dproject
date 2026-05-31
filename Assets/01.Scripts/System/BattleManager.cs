using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance; // 싱글턴 선언

    [Header("게임 난이도 설정")]
    // 난이도 1 = 쉬움, 2 = 보통, 3 = 어려움
    [Range(1, 3)] public int CurrentDifficultyLevel = 1; // 현재 난이도 저장
    // 난이도 따른 데미지 상승률 저장 = 0.5;
    [SerializeField] private float difficultyDamageMultiplier = 0.5f; 

    private void Awake()
    {
        if (Instance == null) // 만약 인스턴스가 널이면
        {
            Instance = this; // 인스턴스는 자기자신
        }
        else // 아니면
        {
            Destroy(gameObject); // 게임오브젝트 삭제
        }
    }

    // 플레이어가 총으로 적을 맞췄을 때 호출되는 함수
    // 적 스탯 정보, 공격 데미지량, 데미지 타입
    public void ProcessPlayerAttack(Enemy_StatManager enemyStat, int rawDamage, DamageType damageType)
    {
       int finalDamage = rawDamage; // 공격 데미지 저장

         // 적 타입이 일반 좀비이고, 대미지타입이 저격총이거나 근접공격이면
       if (enemyStat.CurrentType == ZombieType.Normal &&
            (damageType == DamageType.Sniper || damageType == DamageType.Melee))
        {
            UtillLogRemove.Log($"{damageType}에 일반 좀비 즉사");
            finalDamage = 99999; // 즉사 대미지로 저장
        }

       // 적에게 받을 최종대미지 전달
       enemyStat.TakeDamage(finalDamage); 
    }

    // 적이 플레이어를 공격 했을 때 호출되는 함수
    public void ProcessEnemyAttack(Player_Character player, int rawEnemyAttack)
    {
        if (player == null) return; // 플레이어가 없으면 반환

        // 게임매니저 인스턴스가 있고, 게임매니저가 배틀 액션 상태가 아니면
        if (GameManager.Instance != null && GameManager.Instance.IsBattleActive == false)
        {
            return; // 반환
        }

        // 난이도에 따른 공격력 증가량 계수 저장
        float multiplier = 1f + (CurrentDifficultyLevel - 1) * difficultyDamageMultiplier;
        // 최종대미지 저장 (받은데미지 * 난이도 따른 공격력 증가량 계수)
        int finalDamage = Mathf.RoundToInt(rawEnemyAttack * multiplier);

        // 플레이어에게 받을 최종대미지 전달
        player.TakeDamage(finalDamage);
    }

    // 플레이어 사망 함수
   public void OnPlayerDead()
    {
        if (GameManager.Instance != null) // 게임 매니저 있으면
        {
            // 게임매니저에 게임오버 호출
           // GameManager.Instance.GameOver();
        }

        UtillLogRemove.Error("GAME OVER");
    } 
}
