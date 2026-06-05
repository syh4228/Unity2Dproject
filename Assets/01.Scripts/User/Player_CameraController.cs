using UnityEngine;

public class Player_CameraController : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField]private Transform _target; // 카메라가 따라 다닐 대상

    [Header("카메라 설정")]
    [SerializeField] private float _CameraSpeed = 0.125f; // 카메라 속도
    // 카메라와 추적 대상의 Z값 거리
    [SerializeField] private Vector3 _CameraDistens = new Vector3(0, 0, -10);

    private Collider2D _mapBounds; // 맵 범위
    private Camera _mainCamera; // 메인 카메라
    private float _minX; // 최소 x 좌표
    private float _maxX; // 최대 X 좌표

    private float _camHeight;
    private float _minY;
    private float _maxY;

    private void Start()
    {
        // 마우스가 활동 자유롭게
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; // 마우스 커서 보임

        _mainCamera = Camera.main; // 카메라 정보 저장
        _camHeight = _mainCamera.orthographicSize;
    }

    private void LateUpdate()
    {
        // 게임매니저에서 배틀중이 아니면 반환
        if (GameManager.Instance.IsBattleActive == false) return;

        // 타겟이 없으면 반환
        if (_target == null) return;

        // 카메라 위치 계산 및 이동
        Vector3 offset = _CameraDistens + new Vector3(0, 1f, 0);
        Vector3 currentPosition = _target.position + offset;

        if (_mapBounds != null) // 맵 범위가 있으면
        {
            // 벗어나지 않을 x 범위 계산
            currentPosition.x = Mathf.Clamp(currentPosition.x, _minX, _maxX);
            currentPosition.y = Mathf.Clamp(currentPosition.y, _minY, _maxY);
        }

        // 카메라 이동 속도 계산
        Vector3 newPosition = Vector3.Lerp(transform.position, currentPosition, _CameraSpeed);

        transform.position = newPosition; // 실제 카메라 이동
    }

    public void SetTarget(Transform SetTarget) // 카메라 타겟이 바뀔때 사용 함수
    {
        _target = SetTarget;

        SetupMapBounds(); // 맵 경계 박스 찾는 함수 호출
    }

    // 맵 경계 박스 찾는 함수
    private void SetupMapBounds()
    {
        // Tag가 MapBounds인 오브젝트를 찾아 저장
        GameObject boundsObj = GameObject.FindGameObjectWithTag("MapBounds");

        if (boundsObj != null) // 경계가 있으면
        {
            // 박스 콜라이더 2D 정보가져와 저장
            _mapBounds = boundsObj.GetComponent<BoxCollider2D>();

            if (_mapBounds != null) // 박스 콜라이더 있으면
            {
                // 높이 계산
                float camHeight = _mainCamera.orthographicSize;
                // 가로 절반 길이 계산
                float camWidth = camHeight * _mainCamera.aspect;

                // 콜라이더 끝부분에서 카메라 화면 절반 크기만큼 뺀 값을 진짜 한계선으로 저장
                _minX = _mapBounds.bounds.min.x + camWidth;
                _maxX = _mapBounds.bounds.max.x - camWidth;

                _minY = _mapBounds.bounds.min.y - _camHeight;
                _maxY = _mapBounds.bounds.max.y + _camHeight;

                UtillLogRemove.Log("카메라 맵 경계 설정 완료!");
            }
        }
        else
        {
            UtillLogRemove.Warning("씬에 'MapBounds' 태그를 가진 오브젝트가 없습니다!");
        }
    }
}
