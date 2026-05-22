using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

// 정적 클래스
// 씬에 오브젝트 배치 없이, 어디서든 GameUtill.함수이름()으로 부를수 있음
public static class GameUtill
{
    // 모든 데이터 일괄 로드
    public static void LoadFullData()
    {
        GameDataManager.Instance.LoadSkillData(GetFullDataPath("Skill"));
        GameDataManager.Instance.LoadCharacterData(GetFullDataPath("Character"));
        GameDataManager.Instance.LoadWeaponData(GetFullDataPath("Weapon"));
        GameDataManager.Instance.LoadCostumeData(GetFullDataPath("Costume"));
    }

    // Json 파일의 경로를 찾아주는 함수
    // 이름만 넘겨주면 진짜 컴퓨터 경로를 완성함
    public static string GetFullDataPath(string dataTableName)
    {
        // 없으면
        if (string.IsNullOrEmpty(dataTableName))
        {
            UtillLogRemove.Log("테이블 이름이 올바르지 않습니다!");
            return string.Empty; // 반환
        }

        // 유니티 폴더 기준으로 파일의 위치 지정
        string relativePath = $"JsonConverter/JsonOutput/{dataTableName}.json";
        // 위에서 지정한 경로를 바탕으로 컴퓨터 내의 정체 경로로 변환
        string fullPath = Path.GetFullPath(relativePath);
        return fullPath; // 반환
    }

    // 전투 데미지 계산 도구 함수 ( 캐릭터 레벨, 레벨당 대미지, 크리티컬 여부)
    public static int CalcCharacterFinalDamage(int curCharacterLevel, int levelPerDamage, bool isCritical)
    {
        // 기본 데미지: (현재 레벨 + 레벨당 데미지 상승량)
        int damagePerLevel = (curCharacterLevel + levelPerDamage);
        // 최종 데미지: 크리티컬이 터졌으면(true) 2배로 뻥튀기하고, 아니면(false) 그대로
        int finalDamage = isCritical ? (damagePerLevel * 2) : damagePerLevel;
        return finalDamage; // 반환
    }
}
