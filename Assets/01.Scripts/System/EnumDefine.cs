using UnityEngine;

// enum 정의 선언

public enum DamageType // 대미지 타입
{
    NormalGun, // 일반
    Shotgun, // 샷건
    Sniper, // 스나이퍼
    Melee, // 근접
    Explosion // 폭발
}

public enum ZombieType // 좀비 타입
{
    Normal, // 일반
    Special // 특수
}

public enum AllState // 행동(애니메이션)
{
    Idle,
    Walk,
    Run,
    Attack,
    Dead,
    Hit,
    Jump,
    Drop, // 아이템 줍기
    UseHeal, // 힐 킷 사용
    UseGrenade, // 슈륙탄 던짐
    Melee, // 근접 공격
    Shove, // 밀치기
    Reload, // 재장전
    UseMD, // 진통제 사용
    UseAD, // 아드 사용
    JumpAttack, // 점프 공격
    Pinned // 마운트 발버둥
}

public enum ESpawnMode
{
    FixedItem,         // 지정한 아이템 무조건 스폰
    RandomGun,         // 총기류 중 랜덤 스폰
    RandomConsumable   // 소모품 중 랜덤 스폰
}
