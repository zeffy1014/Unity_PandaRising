using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace DataBase
{
    // ハイスコア・所持金・各種強化レベルなどのプレーデータ
    [Serializable]
    public class UserData {
        [SerializeField] int highScore;     // ハイスコア
        [SerializeField] int pocketMoney;   // 所持金

        // 各種強化情報
        // 自機関連
        [SerializeField] int shotRapidity;      // みかん連射力
        [SerializeField] int shotPower;         // みかん威力
        [SerializeField] int laserOption;       // レーザー発射オプション数(0-2)
        [SerializeField] int laserPower;        // レーザー威力
        [SerializeField] int fishPower;         // 魚威力(サイズも連動)
        [SerializeField] int fishToughness;     // 魚品質保持期間
        // 家関連
        [SerializeField] int houseDurability;   // 家の耐久力
        [SerializeField] int houseHealingPower; // 家の自然回復力
        // その他
        [SerializeField] int speedMagRange;     // 上昇速度の変化幅
        [SerializeField] int continueCredit;    // コンティニュー可能回数

        // 情報取得IF

        // 情報更新IF

        // 初期データ作成
        public void MakeInitialData(string fileName)
        {
            // 初期値はこうする
            shotRapidity = 1;
            shotPower = 1;
            laserOption = 0;
            laserPower = 0;
            fishPower = 1;
            fishToughness = 1;
            houseDurability = 1;
            houseHealingPower = 1;
            speedMagRange = 1;
            continueCredit = 3;

            StreamWriter writer;
            string jsonstr = JsonUtility.ToJson(this);
            writer = new StreamWriter(fileName, false);
            writer.Write(jsonstr);
            writer.Flush();
            writer.Close();

        }

    }
}