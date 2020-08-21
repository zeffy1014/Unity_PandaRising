using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
        // Debug.Log(dirPath);
        return dirPath;
    }
}

