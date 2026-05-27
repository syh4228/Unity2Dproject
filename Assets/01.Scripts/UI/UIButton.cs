using UnityEngine;
using UnityEngine.UI;
using System;

public class UIButton : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private Button Button_Base; // 버튼 연결
    [SerializeField] private Text Text_Base; // 텍스트 연결
    [SerializeField] private Image Image_Base; // 배경 이미지 연결
    [SerializeField] private Image Image_Select; // 버튼 클릭 연출 이미지 연결

    private void Awake()
    {
        // 1-2) 이 오브젝트가 생성될 때, 한번 컴포넌트를 찾아서 캐싱하자
        InitUIButton(); // 버튼 연결 안되 있을시 버튼 찾는 함수 호출
        SetDefaultUI(); // 기본값으로 초기화 하는 함수 호출
    }

    private void OnEnable()
    {
        // 버튼 이벤트 함수 호출
        BindOnClickButtonEvent(OnClickSetSelectUI);
    }

    private void OnDisable()
    {
        // UI가 꺼질때 모든 이벤트를 지워주는 함수 호출
        Button_Base.onClick.RemoveAllListeners();
    }


    private void SetDefaultUI() // 기본 상태 세팅 함수
    {
        if (Image_Select != null)
        {
            Image_Select.gameObject.SetActive(false);
        }
    }

    private void InitUIButton() // 버튼 컴포넌트 자동 탐색 함수
    {
        if (Button_Base != null)
        {
            return;
        }

        // 외부에서도 등록할 수 있고,
        // 누군가 누락했다면 등록안해도 알아서 찾아주는 로직
        var button = this.gameObject.GetComponentInChildren<Button>();

        if (button != null) // 널이 아니면
        {
            this.Button_Base = button; // 그 버튼을 버튼베이스로 저장
        }
    }


    // 외부에서 버튼 누르면 행동을 추가하는 함수
    public void BindOnClickButtonEvent(Action onClickCallback)
    {
        if (Button_Base == null) return;

        // 버튼의 onClick 리스트에, 밖에서 전달받은 행동(onClickCallback)을 추가(AddListener)함
        Button_Base.onClick.AddListener(new UnityEngine.Events.UnityAction(onClickCallback));

    }

    // 외부에서 특정 행동을 버튼 이벤트에서 빼고(제거하고) 싶을 때 쓰는 함수
    public void UnBindOnClickButtonEvent(Action onClickCallback)
    {
        if (Button_Base == null) return;

        // 추가했던 그 특정 행동만 찾아서 다시 지워줌 (RemoveListener)
        Button_Base.onClick.RemoveListener(new UnityEngine.Events.UnityAction(onClickCallback));
    }

    // 버튼 위에 적힌 글씨를 코드로 바꿀 때 쓰는 함수
    public void ChangeButtonText(string buttonStr)
    {
        // 혹시 이버튼을 동적으로, 코드에서 텍스트를 수정해야할 때 사용
        if (Text_Base == null) return;

        // 텍스트 컴포넌트의 글자를 전달받은 문자열(buttonStr)로 덮어쓰기
        Text_Base.text = buttonStr;
    }

    // 버튼이 눌렸을 때 스스로 '선택 이미지(하이라이트)'를 껐다 켰다 하는 함수
    private void OnClickSetSelectUI()
    {
        if (Image_Select != null)
        {
            // 현재 선택 이미지가 눈에 보이고 있는지(true), 꺼져 있는지(false) 상태를 알아내서 변수에 저장
            bool currentActive = Image_Select.gameObject.activeSelf;
            // 느낌표(!)를 붙여서 현재 상태를 반대로 뒤집어서 세팅 (토글 스위치 논리)
            // 꺼져있었으면(!false = true) 켜지고, 켜져있었으면(!true = false) 꺼짐
            Image_Select.gameObject.SetActive(!currentActive);
        }
    }
}
