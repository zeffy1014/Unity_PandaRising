using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace DataBase
{
    public class DataLibrarian
    {
        // データ読み込みできているかどうか
        bool loadOK = false;

        // 保持するデータと在り処
        UniversalData universalData;
        string universalDataPath = "UniversalData";  // Resource/UniversalData.json
        UserData userData;
        string userDataPath = CommonUtils.GetLocalDataDirectory() + "/user.json";  // TODO:暗号化

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
            // データを読み込む 成功なら読み込みOKとして以降のデータ取得が可能
            loadOK = Load();

        }

        // データ読み込み
        private bool Load()
        {
            string dataStr;

            try
            {
                // UniversalData読み込み
                dataStr = Resources.Load(universalDataPath).ToString();
                if (null != dataStr)
                {
                    universalData = JsonUtility.FromJson<UniversalData>(dataStr);
                }
                else
                {
                    // 読み込めなかった場合は保護処理として初期ファイル作成しつつテスト用の値を格納する
                    Debug.Log("UniversalData load failed... make init data...");
                    if (false == MakeInitUniversalData())
                    {
                        // これもだめなら抜ける
                        Debug.Log("DataLibrarian load failed... cannnot make initial UniversalData.");
                        return false;
                    }
                }

                // UserData読み込み
                if (!File.Exists(userDataPath))
                {
                    // ファイルが無かったら初期値設定+ファイル作成
                    userData = new UserData();
                    if (false == userData.MakeInitialData(userDataPath))
                    {
                        // これもだめなら抜ける
                        Debug.Log("DataLibrarian load failed... cannnot make initial UserData.");
                        return false;
                    }
                }
                else
                {
                    // ファイルがあったら読み込み
                    dataStr = File.ReadAllText(userDataPath);
                    userData = JsonUtility.FromJson<UserData>(dataStr);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.Log("DataLibrarian Load failed... exception:" + e);
                return false;
            }
        }

        // UniversalData初期ファイル作成(通常は使用しない)
        private bool MakeInitUniversalData()
        {
            string initStr = CommonUtils.GetExecDirectory() + "/init.json";
            universalData = new UniversalData();
            bool ret = universalData.MakeTestJson(initStr);

            return ret;
        }

        // UserDataファイル更新
        public bool UpdateUserData(UserData newData)
        {
            bool ret = userData.UpdateData(newData, userDataPath);
            return ret;
        }

        /*****各種情報取得IF 読み込み失敗時はnullが返る**********************************/
        // ステージ構成情報取得
        public StageInfo GetStageInfo(StageNumber stage)
        {
            Debug.Log("Get StageInfo stage: " + stage.ToString());
            if (loadOK)
            {
                return universalData.GetStageInfo(stage);
            }
            else
            {
                Debug.Log("DataLibrarian Load Data NG -> cannot get StageInfo...");
                return null;
            }
        }

        // 強化レベル対応パラメータテーブル取得
        public ReinforcementTableInfo GetReinforcementTableInfo()
        {
            if (loadOK)
            {
                return universalData.GetReinforcementTableInfo();
            }
            else
            {
                Debug.Log("DataLibrarian Load Data NG -> cannot get ReinforcementTableInfo...");
                return null;
            }
        }

        // プレー情報(UserData)取得
        public UserData GetUserData()
        {
            if (loadOK)
            {
                return this.userData;
            }
            else
            {
                Debug.Log("DataLibrarian Load Data NG -> cannot get UserData...");
                return null;
            }
        }

        // 敵Prefab格納パス取得
        public string GetEnemyPrefabPath(Enemy.EnemyType type)
        {
            string retStr = universalData.GetEnemyPrefabPath(type);
            Debug.Log("Get EnemyPrefabPath Type:" + type + ", Path:" + retStr);

            if (loadOK)
            {
                return retStr;
            }
            else
            {
                Debug.Log("DataLibrarian Load Data NG -> cannot get EnemyPrefabPath...");
                return null;
            }
        }
    }
}