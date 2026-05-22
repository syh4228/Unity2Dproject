using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; set; } // 싱글턴 선언

    private void Awake()
    {
        Instance = this; // 인스턴스
    }

    [Serializable]
    // 유니티에서 JSON을 직접 읽지 못해서, 배열의 형태의 껍대기를 씌워주는 크래스
    // JsonUtility의 한계를 극복하기 위한 Wrapper
    private class SerializationWrapper<T>
    {
        public List<T> items; // 파일의 루트 키 이름을 지정
    }

    // 딕셔너리에 정보 받아오기
    public Dictionary<string, CharacterData> CharacterDataList { get; private set; }
    public Dictionary<string, SkillData> SkillDataList { get; private set; }
    public Dictionary<string, WeaponData> WeaponDataList { get; private set; }
    public Dictionary<string, CostumeData> CostumeDataList { get; private set; }

    private GameDataManager() // 받아온 정보 변수로 저장
    {
        CharacterDataList = new Dictionary<string, CharacterData>();
        SkillDataList = new Dictionary<string, SkillData>();
        WeaponDataList = new Dictionary<string, WeaponData>();
        CostumeDataList = new Dictionary<string, CostumeData>();
    }

    // 제네릭(T)을 사용하여 모든 데이터 타입을 한 번에 처리 함수
    private Dictionary<string, T> LoadData<T>(string jsonPath) where T : GameDataBase
    {
        if (!File.Exists(jsonPath)) // 파일이 없다면
        {
            UtillLogRemove.Error($"[Error] 파일을 찾을 수 없습니다: {jsonPath}");
            return new Dictionary<string, T>(); // 딕셔너리에 새로 저장
        }
        try // 있으면
        {
            string jsonString = File.ReadAllText(jsonPath); // 문자열로 파일 가져오기
            // Json을 읽을 수 있도록 배열의 형태의 껍대기를 씌워주기
            string wrappedJson = "{\"items\":" + jsonString + "}"; 
            // 리스트 형태로 변환
            SerializationWrapper<T> wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

            if (wrapper != null && wrapper.items != null) // 둘다 했다면
            {
                UtillLogRemove.Log($"{typeof(T).Name} 데이터를 {wrapper.items.Count}개 로드했습니다.");
                // Id 속성의 키값을 아이템으로 변환하여 딕셔너리 저장
                return wrapper.items.ToDictionary(item => item.Id);
            }
        }
        catch (Exception ex) // 실패시
        {
            UtillLogRemove.Error($"[{typeof(T).Name} JSON 로드 오류] {ex.Message}");
        }

        return new Dictionary<string, T>(); // 딕셔너리 반환
    }

    // 각 데이터 타입을 로드
    public void LoadSkillData(string jsonPath)
    {
        SkillDataList = LoadData<SkillData>(jsonPath);
    }

    public void LoadCharacterData(string jsonPath)
    {
        CharacterDataList = LoadData<CharacterData>(jsonPath);
    }

    public void LoadWeaponData(string jsonPath)
    {
        WeaponDataList = LoadData<WeaponData>(jsonPath);
    }

    public void LoadCostumeData(string jsonPath)
    {
        CostumeDataList = LoadData<CostumeData>(jsonPath);
    }

    // Id 를 이용하여 데이터 찾아오기
    public CharacterData GetCharacterData(string id)
    {
        // 만약 리스트가 있고, Id 면 반환
        if (CharacterDataList == null || string.IsNullOrEmpty(id)) return null;
        // 딕셔너리(리스트)에서 id 를 찾고 맞으면 item에 저장 가져오기, 못찾으면 반환
        return CharacterDataList.TryGetValue(id, out var item) ? item : null;
    }

    public SkillData GetSkill(string id)
    {
        if (SkillDataList == null || string.IsNullOrEmpty(id)) return null;
        return SkillDataList.TryGetValue(id, out var item) ? item : null;
    }

    public WeaponData GetWeaponData(string id)
    {
        if (WeaponDataList == null || string.IsNullOrEmpty(id)) return null;
        return WeaponDataList.TryGetValue(id, out var data) ? data : null;
    }

    public CostumeData GetCostumeData(string id)
    {
        if (CostumeDataList == null || string.IsNullOrEmpty(id)) return null;

        return CostumeDataList.TryGetValue(id, out var data) ? data : null;
    }
}