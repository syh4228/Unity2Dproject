using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    public GameObject bulletPrefab; // 발사할 총알 프리팹
    public Transform firePoint;     // 총구 위치
    public int poolSize = 10; // 만들어 놓을 총알 개수

    private List<GameObject> bulletPool; // 생성한 총알 저장소

    private void Start()
    {
        bulletPool = new List<GameObject>(); // 총알 저장

        for (int i = 0; i < poolSize; i++) // 총알 생성
        {
            GameObject bulletObj = Instantiate(bulletPrefab); // 총알 프리팹 가져오기

            bulletObj.SetActive(false); // 만든 총알 비활성화
            bulletPool.Add(bulletObj); // 총알 저장
        }
    }

    // 총알 발사 함수
    public void FireBullet(bool isLookLeft)
    {
        // 만약 총알 프리팹이 없거나, 총 쏘는 포인트가 없다면
        if (bulletPrefab == null || firePoint == null)
        {
            return; // 반환
        }

        GameObject targetBullet = null; // 발사 총알이 없으면

        foreach (GameObject bullet in bulletPool) // 저장소에서 비활성화 총알 찾기
        {
            if (bullet.activeSelf == false) // 만약 비활성화 총알이 있으면
            {
                targetBullet = bullet; // 발사 총알 지정
                break;
            }
        }

        if (targetBullet == null) // 만약 총알이 없으면
        {
            targetBullet = Instantiate(bulletPrefab); // 총알 프리팹 가져오기
            bulletPool.Add(targetBullet); // 저장소에 총알 추가
        }

        targetBullet.transform.position = firePoint.position; // 찾은 총알을 발사위치로 이동
        targetBullet.SetActive(true); // 총알 활성화

        // 총알 매니저 컴포넌트 가져오기
        BulletManager bulletScript = targetBullet.GetComponent<BulletManager>();
        if (bulletScript != null) // 만약 컴포넌트가 있으면
        {
            // 총알 방향 값은 왼쪽 방향이면 -1, 오른쪽이면 +1
            float dirX = isLookLeft ? -1f : 1f;
            bulletScript.SetDirection(dirX); // 총알 방향 함수 호출
        }
    }
}
