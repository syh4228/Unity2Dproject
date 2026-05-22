using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameDataBase
{
    public string Id;
}

[System.Serializable]
public class CharacterData : GameDataBase
{
    public string Name;
    public string SkillList;
    public string UseWeaponId;
    public string BasicCostumeId;
}

[System.Serializable]
public class SkillData : GameDataBase
{
    public string Name;
    public string Description;
}

[System.Serializable]
public class WeaponData : GameDataBase
{
    public string Name;
    public string Description;
}

[System.Serializable]
public class CostumeData : GameDataBase
{
    public string Name;
    public string Description;
}