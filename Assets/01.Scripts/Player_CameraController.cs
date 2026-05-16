using UnityEngine;

public class Player_CameraController : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField]private Transform _target; // 카메라가 따라 다닐 대상

    [Header("카메라 설정")]
    [SerializeField] private float _CameraSpeed = 0.125f; // 카메라 속도
    // 카메라와 추적 대상의 Z값 거리
    [SerializeField] private Vector3 _CameraDistens = new Vector3(0, 0, -10);

    private void Start()
    {
        // 마우스가 활동 자유롭게
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; // 마우스 커서 보임

        if (_target == null )
        {
            UtillLogRemove.Warning("카메라 추적할 대상 연결 확인 요망!");
        }
    }

    private void CameraMoveUpdate()
    {
        if( _target == null )
        {
            return;
        }

        // 카메라 위치 계산
        Vector3 currentPosition = _target.position + _CameraDistens;

        // 카메라 타겟따라 움직이는 속도 계산
        Vector3 newPosition = Vector3.Lerp(transform.position, currentPosition, _CameraSpeed);

        transform.position = newPosition; // 실제 카메라 이동
    }

    public void SetTarget(Transform newTarget) // 카메라 타겟이 바뀔때 사용 함수
    {
        _target = newTarget;
    }
}
