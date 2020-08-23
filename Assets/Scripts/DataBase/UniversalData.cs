using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace DataBase
{
    // ステージ番号定義
    public enum StageNumber
    {
        Tutorial,
        Stage1,
        Stage2,
        Stage3,
        Stage4,
        Stage5,

        Stage_Num
    }

    // 各ステージの構成情報
    [Serializable]
    public class StageInfo
    {
        [SerializeField] StageNumber stage;

        [SerializeField] string pathEnemyGenerateTable;    // 敵生成テーブルの格納パス
        [SerializeField] string pathBackGroundImage;       // 背景画像の格納パス
        [SerializeField] string pathBackGroundMusicStage;  // BGMの格納パス(ステージ)
        [SerializeField] string pathBackGroundMusicBoss;   // BGMの格納パス(ボス戦)

        [SerializeField] int heightStart;               // Start高度
        [SerializeField] int heightGoal;                // Goal高度

        // コンストラクタ 初期データ作成用
        public StageInfo(StageNumber stage, string pathEGT, string pathBGI, string pathBGMS, string pathBGMB, int heightS, int heightG)
        {
            this.stage = stage;
            this.pathEnemyGenerateTable = pathEGT;
            this.pathBackGroundImage = pathBGI;
            this.pathBackGroundMusicStage = pathBGMS;
            this.pathBackGroundMusicBoss = pathBGMB;
            this.heightStart = heightS;
            this.heightGoal = heightG;
        }

    }

    // 各強化レベルに対する具体的なパラメータ(強化レベルがそのまま値になるものは扱わない)
    [Serializable]
    public class ReinforcementTableInfo
    {
        [SerializeField] float[] shotRapidityTable;     // みかん連射性能(次弾発射までの待ち時間)
        [SerializeField] int[] shotPowerTable;          // みかん威力(1発のダメージ)
        [SerializeField] float[] laserPowerTable;       // 果汁レーザー威力(1秒照射あたりのダメージ)
        [SerializeField] int[] fishPowerTable;          // 魚威力
        [SerializeField] float[] fishSizeTable;         // 魚サイズ(倍率)
        [SerializeField] float[] fishToughnessTable;    // 魚品質保持力(時間)
        [SerializeField] int[] houseDurabilityTable;    // 家の耐久力(最大ライフ)
        [SerializeField] int[] houseHealingPowerTable;  // 家の自然回復力(単位時間の回復量)
        [SerializeField] float[] speedMaxMagRangeTable; // 上昇速度上限
        [SerializeField] float[] speedMinMagRangeTable; // 上昇速度下限

        // 初期データ作成
        public void MakeInitTable() {
            shotRapidityTable = new float[5] { 0.2f, 0.18f, 0.15f, 0.11f, 0.07f };
            shotPowerTable = new int[5] { 100, 120, 150, 190, 240 };
            laserPowerTable = new float[5] { 100, 120, 150, 190, 240 };
            fishPowerTable = new int[3] { 250, 350, 500 };
            fishSizeTable = new float[3] { 1.0f, 1.2f, 1.5f };
            fishToughnessTable = new float[3] { 3.0f, 4.0f, 5.0f };
            houseDurabilityTable = new int[5] { 1000, 1200, 1600, 2400, 3500 };
            houseHealingPowerTable = new int[5] { 1, 2, 3, 4, 5 };
            speedMaxMagRangeTable = new float[3] { 1.5f, 2.0f, 3.0f };
            speedMinMagRangeTable = new float[3] { 0.7f, 0.5f, 0.3f };

            return;
        }
    }

    // ゲーム全体を通じて不変となる情報 JSONで読み込む
    [Serializable]
    public class UniversalData
    {
        [SerializeField] StageInfo[] stageInfo;
        [SerializeField] ReinforcementTableInfo rtInfo;

        // 情報取得IF
        public StageInfo GetStageInfo(StageNumber stage)
        {
            if (stageInfo.Length > (int)stage)
            {
                return stageInfo[(int)stage];
            }
            else
            {
                Debug.Log("stage number is out of range... Max:" + stageInfo.Length + ", Selected:" + (int)stage);
                return null;
            }
        }

        // for Make TestData
        public void MakeTestJson(string filePath)
        {
            // StageInfo初期データ
            stageInfo = new StageInfo[(int)StageNumber.Stage_Num];
            stageInfo[(int)StageNumber.Tutorial] = new StageInfo(StageNumber.Tutorial, "path", "path", "path", "path", 0, 1);
            stageInfo[(int)StageNumber.Stage1] = new StageInfo(StageNumber.Stage1, "path1", "path1", "path1", "path1", 0, 2000);
            stageInfo[(int)StageNumber.Stage2] = new StageInfo(StageNumber.Stage2, "path2", "path2", "path2", "path2", 2000, 10000);
            stageInfo[(int)StageNumber.Stage3] = new StageInfo(StageNumber.Stage3, "path3", "path3", "path3", "path3", 10000, 50000);
            stageInfo[(int)StageNumber.Stage4] = new StageInfo(StageNumber.Stage3, "path4", "path4", "path4", "path4", 50000, 200000);
            stageInfo[(int)StageNumber.Stage5] = new StageInfo(StageNumber.Stage5, "path5", "path5", "path5", "path5", 200000, 1000000);

            // ReinforcementTableInfo初期データ
            rtInfo = new ReinforcementTableInfo();
            rtInfo.MakeInitTable();

            StreamWriter writer;
            string jsonstr = JsonUtility.ToJson(this);
            //string filePath = Application.dataPath + "/" + fileName;
            writer = new StreamWriter(filePath, false);
            writer.Write(jsonstr);
            writer.Flush();
            writer.Close();

        }


    }
}