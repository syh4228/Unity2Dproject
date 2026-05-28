using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// 정적 클래스
// 씬에 오브젝트 배치 없이, 어디서든 GameUtill.함수이름()으로 부를수 있음
public static class GameUtill
{
    // 마지막으로 할당된 ID를 전역적으로 기록 (스레드 안전)
    private static long _lastId = 0;

    // 모든 데이터 일괄 로드
    public static void LoadFullData()
    {
        GameDataManager.Instance.LoadSkillData("Skill");
        GameDataManager.Instance.LoadCharacterData("Character");
        GameDataManager.Instance.LoadWeaponData("Weapon");
        GameDataManager.Instance.LoadCostumeData("Costume");
        GameDataManager.Instance.LoadDNMonsterData("DNMonster");
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

    public static Sprite LoadSpriteCanBeNull(string spriteName)
    {
        // 1. Resources/ 경로에서 이름으로 스프라이트 로드
        // 예: spriteName이 "Sword"라면 Assets/Resources/2D/Sword.png를 찾음
        // 이 2D같은 경로는 나중에 Sprite, Texture 등등 다양하게 바꿔도 무관합니다!
        Sprite loadedSprite = Resources.Load<Sprite>($"{spriteName}");

        if (loadedSprite != null)
        {
            return loadedSprite;
        }

        Debug.LogError($"에셋을 찾을 수 없습니다: {spriteName}");
        return null;
    }

    public static async UniTask<Sprite> LoadAndSetSpriteImage(Image targetImage, string spritePath)
    {
        Sprite sprite = await ResourceManager.Inst.LoadSprite(spritePath);
        if (sprite != null)
        {
            targetImage.sprite = sprite;
        }
        return sprite;
    }

    public static async UniTaskVoid LoadAndPlayAudioClip(AudioSource audioSource, string audioPath, bool isLoop = false)
    {
        AudioClip clip = await ResourceManager.Inst.LoadAsset<AudioClip>(audioPath);
        if (clip == null)
        {
            Debug.LogError($"{audioPath}를 찾을 수 없습니다! 어드레서블 설정이 되어 있는지 확인해주세요.");
            return;
        }

        if (isLoop == true)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public static async UniTaskVoid LoadAndSetTexture(RawImage targetRawImage, string texturePath)
    {
        // 비동기로 로드하기 전까지는 해당 오브젝트를 잠깐 비활성화 해준다
        targetRawImage.gameObject.SetActive(false);
        Texture texture = await ResourceManager.Inst.LoadAsset<Texture>(texturePath);
        if (texture != null)
        {
            targetRawImage.texture = texture;
        }
        targetRawImage.gameObject.SetActive(true);
    }

    public static List<string> GetDialogueIdList(string dialogueGroupId)
    {
        var list = new List<string>();

        // "dialogue_group_mainstream_1_1"
        var data = GameDataManager.Instance.GetDNDialogueGroupData(dialogueGroupId);
        if (data != null)
        {
            var idArr = data.DialogueIdList;
            foreach (var id in idArr)
            {
                list.Add(id);
            }
        }

        return list;
    }

    // 그냥 유니크 키가 발급되어야 할 때 사용하려고 만든 것 (의미가 있는 건 아니므로 사용만 하세요)
    public static long GenerateUniqueId()
    {
        long newId = DateTime.UtcNow.Ticks;

        // 원자적 연산으로 안전하게 ID 갱신
        while (true)
        {
            long lastId = Volatile.Read(ref _lastId);

            // 만약 현재 시간이 이전 ID보다 작거나 같다면 (루프가 너무 빠른 경우 포함)
            // 이전 ID + 1로 강제 설정하여 중복 방지
            long idToAssign = (newId <= lastId) ? lastId + 1 : newId;

            // _lastId가 내가 읽은 시점과 같다면 idToAssign으로 교체 (성공 시 루프 탈출)
            if (Interlocked.CompareExchange(ref _lastId, idToAssign, lastId) == lastId)
            {
                return idToAssign;
            }
            // 그 사이 다른 스레드가 값을 바꿨다면 다시 시도
        }
    }
}
