using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    public GameObject bulletPrefab; // 발사할 총알 프리팹
    public Transform firePoint;     // 총구 위치
    public int poolSize = 10; // 만들어 놓을 총알 개수

    private List<GameObject> bulletPool; // 생성한 총알 저장소

    private float _nextFireTime = 0f; // 연사 쿨 저장 변수

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
    public bool FireBullet(bool isLookLeft, int weaponDamage, DamageType weaponType, float effectiveRange, float rpm)
    {
        float realRPM = rpm * 10f; // 실제 RPM 보정해서 저장

        // 60 나누기 실제 RPM 값을 발사딜레이에 저장
        float fireDelay = 60f / realRPM;

        // 현재 시간이 다음 사격가능 시간보다 작으면
        if (Time.time < _nextFireTime)
        {
            return false; // 반환, 실패
        }

        // 현재시간에 발사딜레이 더한 시간을 다음 사격가능 시간에 저장
        _nextFireTime = Time.time + fireDelay;

        // 만약 총알 프리팹이 없거나, 총 쏘는 포인트가 없다면
        if (bulletPrefab == null || firePoint == null)
        {
            return false; // 반환, 실패
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
            // 바라보는 방향, 대미지, 무기타입 저장, 사거리 저장
            bulletScript.SetDirection(dirX, weaponDamage, weaponType, effectiveRange); // 총알 방향 함수 호출
        }

        return true; // 반환, 발사 성공
    }
}
