using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace DataBase
{
    // 強化対象
    public enum ReinforceTarget
    {
        // 自機関連
        ShotRapidity,      // みかん連射力
        ShotPower,         // みかん威力
        LaserOption,       // レーザー発射オプション数(0-2)
        LaserPower,        // レーザー威力
        FishPower,         // 魚威力(サイズも連動)
        FishToughness,     // 魚品質保持期間
        // 家関連
        HouseDurability,   // 家の耐久力
        HouseHealingPower, // 家の自然回復力
        // その他
        SpeedMagRange,     // 上昇速度の変化幅
        ContinueCredit,    // コンティニュー可能回数

        Target_Num
    }

    // ハイスコア・所持金・各種強化レベルなどのプレーデータ
    [Serializable]
    public class UserData {
        [SerializeField] int highScore;     // ハイスコア
        [SerializeField] int pocketMoney;   // 所持金
        [SerializeField] int[] reinforcementLevel = new int[(int)ReinforceTarget.Target_Num];　 // 各種強化情報

        // 情報取得IF
        public int GetLevel(ReinforceTarget target)
        {
            if (reinforcementLevel.Length > (int)target)
            {
                return reinforcementLevel[(int)target];
            }
            else
            {
                // 取得失敗時は-1が返る
                Debug.Log("Target:" + target + " is out of range...");
                return -1;
            }
        }

        public int GetHighScore() { return highScore; }
        public int GetPocketMoney() { return pocketMoney; }

        // ファイル更新
        public bool UpdateData(UserData newData, string fileName)
        {
            try
            {
                StreamWriter writer;
                string jsonStr = JsonUtility.ToJson(newData);
                using (writer = new StreamWriter(fileName, false))
                {
                    writer.Write(jsonStr);
                    writer.Flush();
                    writer.Close();
                }
                // 書き込み成功
                return true;
            }
            catch (Exception e)
            {
                // 書き込み失敗orz
                Debug.Log("Update UserData failed... Exception:" + e);
                return false;
            }

        }

        // 初期データ作成
        public bool MakeInitialData(string fileName)
        {
            // 初期値はこうする
            highScore = 0;
            pocketMoney = 0;
            reinforcementLevel[(int)ReinforceTarget.ShotRapidity] = 1;
            reinforcementLevel[(int)ReinforceTarget.ShotPower] = 1;
            reinforcementLevel[(int)ReinforceTarget.LaserOption] = 0;
            reinforcementLevel[(int)ReinforceTarget.LaserPower] = 1;
            reinforcementLevel[(int)ReinforceTarget.FishPower] = 1;
            reinforcementLevel[(int)ReinforceTarget.FishToughness] = 1;
            reinforcementLevel[(int)ReinforceTarget.HouseDurability] = 1;
            reinforcementLevel[(int)ReinforceTarget.HouseHealingPower] = 1;
            reinforcementLevel[(int)ReinforceTarget.SpeedMagRange] = 1;
            reinforcementLevel[(int)ReinforceTarget.ContinueCredit] = 3;

            try
            {
                StreamWriter writer;
                string jsonstr = JsonUtility.ToJson(this);
                using (writer = new StreamWriter(fileName, false))
                {
                    writer.Write(jsonstr);
                    writer.Flush();
                    writer.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Cannot make initial UserData... Exeption:" + e);
                return false;
            }

        }

    }
}