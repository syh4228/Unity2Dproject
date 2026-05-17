using UnityEngine;

public static class UtilRemove
{
    // 컴포넌트 파괴하는 함수
    public static void RemoveComponent<T>(this GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component != null)
        {
            UtillLogRemove.Warning($"{obj.name} 오브젝트에서 {typeof(T).Name} 컴포넌트가 제거되었습니다.");
            Object.Destroy(component);
        }
    }

    // 오브젝트 파괴하는 함수
    public static void DestroySafe(this GameObject obj)
    {
        if (obj != null)
        {
            UtillLogRemove.Warning($"{obj.name} 오브젝트가 게임에서 완전히 파괴되었습니다.");
            Object.Destroy(obj);
        }
    }

    // 오브젝트 비활성화 하는 함수
    public static void DeactivateSafe(this GameObject obj)
    {
        if (obj != null)
        {
            UtillLogRemove.Warning($"{obj.name} 오브젝트가 비활성화(풀 반환) 되었습니다.");
            obj.SetActive(false);
        }
    }
}
