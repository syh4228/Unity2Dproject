using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameDataBase
{
    public string Id;
}

// Syste.Text.Json대신 유니티 내장 JsonUtility를 사용
// 따라서 프로퍼티말고 그냥 일반 public 멤버변수로 변경함
// [System.Serializable]가 없다면 JsonUtility는 데이터를 무시

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
    public int Damage;
    public int RPM;
    public int EffectiveRange;
    public int Capacity;
    public int Capacity2;
    public string IconPath;
    public string PrefabPath;
    public string UseType;
    public string Anim_AttackPath;
    public string Anim_ReloadPath;
    public string Type;
}

[System.Serializable]
public class CostumeData : GameDataBase
{
    public string Name;
    public string Description;
}

[System.Serializable]
public class DNDialogueGroupData : GameDataBase
{
    public List<string> DialogueIdList;
}

[System.Serializable]
public class DNDialogueData : GameDataBase
{
    public string CharacterDataId;
    public string Description;
    public string NextDialogueId;
    public List<string> SelectionNameList;
    public List<string> SelectionDialogueIdList;
    public string TexturePath;
    public string VoicePath;
}

[System.Serializable]
public class DNMonsterData : GameDataBase
{
    public string Name;
    public string Description;
    public int BaseHp;
    public int BaseAtk;
    public float NormalAtkMultiple;
    public List<float> SkillAtkMultipleList;
    public string IconPath;
    public string PrefabPath;
    public string Type;
}