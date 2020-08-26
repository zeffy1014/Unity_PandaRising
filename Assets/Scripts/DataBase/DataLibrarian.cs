using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DataBase
{
    public class DataLibrarian
    {
        // 初期ファイル生成用フラグ(Test用)
        bool makeInitData = false;

        // 保持するデータと在り処
        UniversalData universalData;
        string universalDataPath = "UniversalData";  // Resource/UniversalData.json
        UserData userData;
        string userDataPath = CommonUtils.GetExecDirectory() + "/user.json";  // TODO:暗号化

        // Singletonにする
        private static DataLibrarian librarianInstance;
        public static DataLibrarian Instance
        {
            get
            {
                if (librarianInstance == null)
                {
                    librarianInstance = new DataLibrarian(); // 生成およびデータ読み込み
                }
                return librarianInstance;
            }
        }

        private DataLibrarian()
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

            // 初期ファイル生成する場合
            if (makeInitData) MakeInitUniversalData();

        }

        // UniversalData初期ファイル作成(通常は使用しない)
        private void MakeInitUniversalData()
        {
            string initStr = CommonUtils.GetExecDirectory() + "/init.json";
            universalData = new UniversalData();
            universalData.MakeTestJson(initStr);

            return;
        }

        /*****各種情報取得IF**********************************************/
        // ステージ構成情報取得
        public StageInfo GetStageInfo(StageNumber stage)
        {
            Debug.Log("Get StageInfo stage: " + stage.ToString());
            return universalData.GetStageInfo(stage);
        }

        // プレー情報取得

        // 敵Prefab格納パス取得
        public string GetEnemyPrefabPath(Enemy.EnemyType type)
        {
            string retStr = universalData.GetEnemyPrefabPath(type);
            Debug.Log("Get EnemyPrefabPath Type:" + type + ", Path:" + retStr);

            return retStr;
        }
    }
}