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
    UseInstantHeal, // 아드, 구급약 사용
    UseGrenade, // 슈륙탄 던짐
    Melee, // 근접 공격
    Shove // 밀치기
}
