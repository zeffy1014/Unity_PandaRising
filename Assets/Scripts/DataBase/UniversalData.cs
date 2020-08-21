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

    // ゲーム全体を通じて不変となる情報 JSONで読み込む
    [Serializable]
    public class UniversalData
    {
        [SerializeField] StageInfo[] stageInfo;

        // for Make TestData
        public void MakeTestJson(string fileName)
        {
            stageInfo = new StageInfo[(int)StageNumber.Stage_Num];

            stageInfo[(int)StageNumber.Tutorial] = new StageInfo(StageNumber.Tutorial, "path", "path", "path", "path", 0, 1);
            stageInfo[(int)StageNumber.Stage1] = new StageInfo(StageNumber.Stage1, "path1", "path1", "path1", "path1", 0, 2000);
            stageInfo[(int)StageNumber.Stage2] = new StageInfo(StageNumber.Stage2, "path2", "path2", "path2", "path2", 2000, 10000);
            stageInfo[(int)StageNumber.Stage3] = new StageInfo(StageNumber.Stage3, "path3", "path3", "path3", "path3", 10000, 50000);
            stageInfo[(int)StageNumber.Stage4] = new StageInfo(StageNumber.Stage3, "path4", "path4", "path4", "path4", 50000, 200000);
            stageInfo[(int)StageNumber.Stage5] = new StageInfo(StageNumber.Stage5, "path5", "path5", "path5", "path5", 200000, 1000000);

            StreamWriter writer;
            string jsonstr = JsonUtility.ToJson(this);
            string filePath = Application.dataPath + "/" + fileName;
            writer = new StreamWriter(filePath, false);
            writer.Write(jsonstr);
            writer.Flush();
            writer.Close();

        }


    }
}