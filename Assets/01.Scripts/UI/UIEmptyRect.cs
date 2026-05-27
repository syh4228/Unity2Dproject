using UnityEngine;
using UnityEngine.UI;

// + 그래픽 렌더링은 하지 않지만 레이캐스트는 받는 컴포넌트
// 버튼의 자식이나, 버튼에 컴포넌트로 붙여서 쓴다
public class UIEmptyRect : Graphic  // Graphic => 클릭을 받을 수 있는 최소한의 자격증
{
    // 업데이트 무시하기 => 유니티 UI는 크기나 색상이 변하면 화면을 다시 그리려고(Dirty, Rebuild) 준비
    // 하지만 내부를 텅 비워서 "나한테는 화면 갱신하라고 명령 내리지 마! 연산 패스해!"
    // 라고 선언하여 CPU 성능을 아낌
    public override void SetAllDirty() { }
    public override void Rebuild(CanvasUpdate update) { }

    // 화면에 이미지를 그리기 위해 폴리곤 덩어리(Mesh)의 점들을 찍어주는 함수
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        // 메쉬를 비워서 아무것도 그리지 않게 함
        vh.Clear();
    }
}
