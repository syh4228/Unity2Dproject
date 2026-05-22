using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 정적 클래스
// 어디서든 GameDataTester.StartDataTest()로 바로 실행
public static class GameDataTester 
{
    public static void StartDataTest()
    {
        // 데이터 전체 로드 함수 호출
        GameUtill.LoadFullData();

        // 단일 데이터 검색 및 딕셔너리 순회 테스트
        // "~" ID를 가진 코스튬 데이터를 가져오기
        var myCostume = GameDataManager.Instance.GetCostumeData("Costume_02");
        UtillLogRemove.Log(myCostume.Name); // 이름을 출력

        // 딕셔너리 전체를 하나씩 꺼내서 확인
        foreach (var kv in GameDataManager.Instance.CostumeDataList)
        {
            string key = kv.Key; // 딕셔너리의 ID 저장
            var data = kv.Value; // 딕셔너리의 데이터 저장
            UtillLogRemove.Log($"키는 {key} 데이터의 이름 : {data.Name} ");
        }

        // "~" 캐릭터의 기본 정보 가져오기
        var myHero = GameDataManager.Instance.GetCharacterData("character_hellena_01");

        if (myHero != null) // 있으면
        {
            UtillLogRemove.Log($"로드된 캐릭터 이름: {myHero.Name}");
        }

        // 캐릭터의 데이터 안에 (BasicoCostumeId = 기본 코스튬) 정보를 꺼내 게임매니저에게 코스튬 데이타를 가져오기
        CostumeData heroCostumeData = GameDataManager.Instance.GetCostumeData(myHero.BasicCostumeId);

        if (heroCostumeData != null) // 있으면
        {
            UtillLogRemove.Log(heroCostumeData.Name);
        }

        // 스킬이 있으면 (스킬이 여러개일 경우 방법)
        if (myHero.SkillList != string.Empty)
        {
            // Split(',') 함수를 사용 ' , ' 를 기준으로 글자들을 쪼개 배열로 저장
            string[] skillNameList = myHero.SkillList.Split(',');

            // 저장된 스킬들 하나씩 꺼내서 확인
            foreach (string skillName in skillNameList)
            {
                // 게임매너제에게 스킬데이터 가져오기
                var skillData = GameDataManager.Instance.GetSkill(skillName);
                // 있으면
                if (skillData != null)
                {
                     UtillLogRemove.Log($"로드된 캐릭터: {myHero.Name}는 {skillData.Name}을 갖고 있다!");
                }
            }
        }

        // 무기(ID, 데이터)가 있으면
        if (string.IsNullOrEmpty(myHero.UseWeaponId) == false)
        {
            // 게임매니저에게 무기 데이터 가져오기
            var weaponData = GameDataManager.Instance.GetWeaponData(myHero.UseWeaponId);
            if (weaponData != null)
            {
                UtillLogRemove.Log($"로드된 캐릭터: {myHero.Name}는 사용무기로 {weaponData.Name}을 갖고 있다!");
            }
        }
    }
}
