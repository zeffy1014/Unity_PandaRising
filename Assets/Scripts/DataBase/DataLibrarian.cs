using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DataBase
{
    public class DataLibrarian
    {
        // 保持するデータと在り処
        UniversalData universalData;
        string universalDataPath = "UniversalData";  // Resource/UniversalData.json
        UserData userData;
        string userDataPath = CommonUtils.GetExecDirectory() + "/user.json";  // TODO:暗号化

        DataLibrarian()
        {
            string dataStr;

            // UniversalData読み込み
            dataStr = Resources.Load(universalDataPath).ToString();
            universalData = JsonUtility.FromJson<UniversalData>(dataStr);

            // UserData読み込み
            if (!File.Exists(userDataPath))
            {
                // ファイルが無かったら初期値設定+ファイル作成
                userData = new UserData();
                userData.MakeInitialData(userDataPath);
            }
            else
            {
                // ファイルがあったら読み込み
                dataStr = File.ReadAllText(userDataPath);
                userData = JsonUtility.FromJson<UserData>(dataStr);
            }

        }

    }
}