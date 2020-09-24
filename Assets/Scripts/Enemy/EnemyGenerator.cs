using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.IO;
using System.Linq;
using DataBase;
using Bullet;

namespace Enemy {

    public enum EnemyType
    {
        // TODO:敵の種類を決めて明記
        Enemy_Fly,
        EnemyB,
        EnemyC,

        EnemyType_Num
    }

    /*** 敵を生成する際の各種情報 ***/
    public class EnemyGenerateInfo
    {
        // 入力されるべき値の数をとりあえず固定値で持っておく
        static readonly int infoNum = 7;

        // カンマ区切りの文字列を順にメンバに格納する
        public EnemyGenerateInfo(string str)
        {
            string[] input = str.Split(',');
            // まずは入力された情報の数をチェック
            if (infoNum != input.Length)
            {
                Debug.Log("Invalid Input Num...Input:" + input.Length + " != " + infoNum);
                return;
            }
            // 超絶決め打ちで格納していく
            /***
             * 文字列の構成
             * id,height,enemy_type,position_x,angle,active_time
             * [0]id:ID(int)
             * [1]enemy_type:何の敵を出すか(EnemyType)
             * [2]height:生成高度(int)
             * [3]position_x:画面上部の出現位置(float)
             * [4]angle:生成角度(float)
             * [5]active_time:活動時間(float)
             * [6]undefeatable_time:出現後の無敵時間(float)
             */
            try
            {
                this.Id = int.Parse(input[0]);

                //this.Type = (EnemyType)Enum.Parse(typeof(EnemyType), input[1]);
                EnemyType type;
                if(!Enum.TryParse<EnemyType>(input[1], out type))
                {
                    type = EnemyType.Enemy_Fly;
                    Debug.Log("EnemyGenerateInfo Parse type error -> set default type:" + type);
                }
                this.Type = type;

                this.GenerateHeight = int.Parse(input[2]);
                this.GeneratePosition = float.Parse(input[3]);
                this.GenerateAngle = float.Parse(input[4]);
                this.ActivityTime = float.Parse(input[5]);
                this.UndefeatableTime = float.Parse(input[6]);

                // ここまで入力できたら情報セット成功とする
                SuccessInput = true;
            }
            catch (Exception e)
            {
                // 何らか入力不正で例外発生した場合
                Debug.Log(e);
                return;
            }
        }

        // 文字列による情報セットが成功しているか
        public bool SuccessInput { get; private set; } = false;

        // 生成が終わった後の削除用フラグ
        public bool DeleteOK { get; set; } = false;

        // 各種生成情報
        public int Id { get; private set; }                 // ユニークなID
        public EnemyType Type { get; private set; }         // 何の敵を出すか
        public int GenerateHeight { get; private set; }     // 生成高度
        public float GeneratePosition { get; private set; } // 生成位置
        public float GenerateAngle { get; private set; }    // 生成角度
        public float ActivityTime { get; private set; }     // 活動時間
        public float UndefeatableTime { get; private set; } // 出現後の無敵時間
    }

    public class EnemyGenerator: ILoadData
    {

        GameController gameController;
        GameArea gameArea;
        Player player;
        BulletGenerator bulletGenerator;

        bool loadTable = false;        // テーブル読み込み完了チェック

        // 敵生成テーブル
        List<EnemyGenerateInfo> generateTable = new List<EnemyGenerateInfo>();
        // 敵生成高度を抽出・重複排除したもの(監視対象のフィルタに使用する)
        List<int> generateHeight = new List<int>();
        // 敵生成高度リストの監視対象番号 生成処理が終わるたびに進む
        int watchHeightIndex = default;
        // 敵のPrefabリスト(生成時に使用、必要なものだけLoadしておく)
        GameObject[] enemyPrefabs = new GameObject[(int)EnemyType.EnemyType_Num];

        /***** 読み込み完了監視 ************************************************************/
        ReactiveProperty<bool> _onLoadCompleteProperty = new ReactiveProperty<bool>(false);
        public IReadOnlyReactiveProperty<bool> OnLoadCompleteProperty => _onLoadCompleteProperty;
        public bool LoadCompleted() { return _onLoadCompleteProperty.Value; }


        /***** 読み込み・準備処理 **********************************************************/
        // コンストラクタ
        EnemyGenerator(GameController gc, GameArea ga, Player pl, BulletGenerator bg)
        {
            gameController = gc;
            gameArea = ga;
            player = pl;
            bulletGenerator = bg;

            // ステージ指定してDataLibrarianからステージ構成情報→敵生成テーブル読み込み
            bool loadResult = LoadTable(DataLibrarian.Instance.GetStageInfo(gc.PlayingStage).GetPathEnemyGenerateTable());
            if (true == loadResult)
            {
                // 読み込み完了したらフラグを立てる
                _onLoadCompleteProperty.Value = true;
            }
            else
            {
                // TODO:読み込み失敗したらエラー通知してメインメニューに戻る？
                Debug.Log("EnemyGenerator load data failed...");
            }

            // Enemyに情報受け渡し
            EnemyBase.SetPlayer(player);
            EnemyBase.SetGameArea(gameArea);
            EnemyBase.SetBulletGenerator(bulletGenerator);
            gameController.SpeedMagReactiveProperty.DistinctUntilChanged().Subscribe(mag => EnemyBase.UpdateSpeedMagnification(mag));

        }

        // 生成テーブル読み込み
        bool LoadTable(string filePath)
        {
            // 指定されたCSVファイルからの読み込み
            TextAsset tableFile = Resources.Load(filePath) as TextAsset;
            if (null == tableFile)
            {
                // 読み込み失敗…
                Debug.Log("EnemyGenerator LoadTable failed...cannot get table file path.");
                return false;
            }

            try
            {
                StringReader sReader = new StringReader(tableFile.text);
                // 1行目を読み飛ばす
                sReader.ReadLine();
                // 2行目以降を格納
                while (sReader.Peek() != -1)
                {
                    // 1行読んで生成情報に格納・リスト追加
                    SetString2EnemyGenerateInfo(sReader.ReadLine());
                }
            }
            catch (Exception e)
            {
                Debug.Log("EnemyGenerator LoadTable failed...Exception:" + e);
                return false;
            }

            // 生成テーブルで指定された高度を監視
            gameController.HeightReactiveProperty
                .DistinctUntilChanged()
                //.Where(height => generateHeight.Contains(height))
                .Subscribe(height =>
                {
                    GenerateEnemy((int)height);
                });

            // 監視対象番号を初期化
            watchHeightIndex = 0;

            return true;
        }

        // 文字列をカンマ区切りでEnemyGenerateInfoに先頭から格納
        bool SetString2EnemyGenerateInfo(string str)
        {
            bool ret = false;

            // 文字列から生成情報を作成
            var genInfo = new EnemyGenerateInfo(str);

            // 正しく情報セットできていたらリストへ追加
            if (genInfo.SuccessInput)
            {
                generateTable.Add(genInfo);                     // 敵生成情報
                if (!generateHeight.Contains(genInfo.GenerateHeight))
                {
                    generateHeight.Add(genInfo.GenerateHeight); // 敵生成情報の高度(重複排除)
                }
                GetEnemyPrefab(genInfo.Type);                   // 敵のPrefabを事前に読み込んでおく

                ret = true;
            }
            else
            {
                Debug.Log("Invalid Input Info...");
            }

            return ret;
        }

        // 敵Prefabを取得(未取得ならLoad)
        GameObject GetEnemyPrefab(EnemyType type)
        {
            if (null == enemyPrefabs[(int)type])
            {
                // DataLibrarianからパス取得してロードする
                enemyPrefabs[(int)type] = (GameObject)Resources.Load(DataLibrarian.Instance.GetEnemyPrefabPath(type));
            }

            return enemyPrefabs[(int)type];
        }

        /***** ゲーム中処理 **********************************************************/
        // 生成テーブルに従って敵を生成
        void GenerateEnemy(int height)
        {
            // 敵生成高度のリストを参照し、その高度に達していなかったら抜ける
            if (generateHeight.Count <= watchHeightIndex)
            {
                // 最後まで参照済みの場合は即抜ける
                return;
            }
            if (generateHeight[watchHeightIndex] > height)
            {
                // まだ敵生成高度に達していないので抜ける
                return;
            }

            // 指定された高度に達しているものを一通り抽出して処理する
            // 生成したらTableから削除する

            Debug.Log("Current height:" + height + ", Next generate enemy height:" + generateHeight[watchHeightIndex] + " -> generate enemy!!");

            // 生成高度が達しているものを抽出
            var generateList = generateTable.Where(info => info.GenerateHeight < height);

            // 抽出したものを順次処理
            foreach (var generateInfo in generateList)
            {
                // Let's生成
                GameObject enemy = UnityEngine.Object.Instantiate(
                    GetEnemyPrefab(generateInfo.Type),
                    gameArea.GetPosFromRate(new Vector2(generateInfo.GeneratePosition, 1.0f)),
                    Quaternion.Euler(0.0f, 0.0f, generateInfo.GenerateAngle -180.0f)
                    );

                // 各種情報を渡してあげる
                enemy.GetComponent<EnemyBase>().SetGenerateInfo(
                    idIn: generateInfo.Id,
                    activityTimeIn: generateInfo.ActivityTime,
                    undefeatableTimeIn: generateInfo.UndefeatableTime
                    );

                // あとでまとめて削除する対象にする
                generateInfo.DeleteOK = true;

            }

            // もともとのListから該当する敵の情報を抜く(ID一致する要素を削除)
            generateTable.RemoveAll(info => true == info.DeleteOK);

            // 監視対象番号を進める
            watchHeightIndex++;

            return;
        }

    }
}