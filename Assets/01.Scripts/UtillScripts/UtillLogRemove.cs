using UnityEngine;
using System.Diagnostics;

public class UtillLogRemove
{
    // 유니티 에디터에서만 실행되고, 빌드에서는 지우기 선언
    [Conditional("UNITY_EDITOR")]
    public static void Log(object msg) // 로그 관리
    {
        UnityEngine.Debug.Log(msg);
    }

    [Conditional("UNITY_EDITOR")]
    public static void Warning(object msg) // 경고 로그 관리
    {
        UnityEngine.Debug.LogWarning(msg);
    }

    [Conditional("UNITY_EDITOR")]
    public static void Error(object msg) // 에러 로그 관리
    {
        UnityEngine.Debug.LogError(msg);
    }
}
