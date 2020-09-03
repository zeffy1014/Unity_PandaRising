using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

// 動作プラットフォーム判断
public class PlatformInfo
{
    static readonly bool isAndroid = Application.platform == RuntimePlatform.Android;
    static readonly bool isIOS = Application.platform == RuntimePlatform.IPhonePlayer;

    public static bool IsMobile()
    {
        // AndroidかiOSか、あるいはUnity RemoteだったらMobile扱いとする
#if UNITY_EDITOR
        bool ret = UnityEditor.EditorApplication.isRemoteConnected;
        // Debug.Log("isRemoteConnected:" + ret);
#else
        bool ret = isAndroid || isIOS;
#endif
        return ret;
    }

}

// 汎用処理
public class CommonUtils
{
    // プログラム実行時のカレントディレクトリを取得
    public static string GetExecDirectory()
    {
        string dirPath;

#if UNITY_EDITOR
        dirPath = Directory.GetCurrentDirectory();
#else
        dirPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
#endif
        Debug.Log("Exec Directory:" + dirPath);
        return dirPath;
    }

    // ローカルに保存するデータ置き場
    static string localDataDirectory = null;
    public static string GetLocalDataDirectory()
    {
        if (null == localDataDirectory)
        {
#if UNITY_EDITOR
            localDataDirectory = GetExecDirectory();
#elif UNITY_ANDROID
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var getFilesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir"))
            {
                // getAbsolutePathまたはgetCanonicalPathで絶対パスを取得
                localDataDirectory = getFilesDir.Call<string>("getCanonicalPath");
            }
#else
            localDataDirectory = GetExecDirectory();
#endif
        }
        Debug.Log("Local Data Directory:" + localDataDirectory);
        return localDataDirectory;
    }
}

