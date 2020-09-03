using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Enemy;


namespace UGUI
{
    public class UIPresenter
    {
        // 操作対象
        private ThrowButtonView tButtonView;
        private BombButtonView bButtonView;
        private PlayerLife pLife;
        private HouseLife hLife;
        private BossLife bLife;
        private SpeedInfo sInfo;
        private HeightInfo hInfo;
        private Combo combo;
        private PlayTime time;
        private Score score;
        private Money money;
        private LoadingView loadingView;

        // 監視対象
        private GameController gameController;
        private Player player;
        private House house;
        private EnemyGenerator enemyGenerator;
        private BGScroller bgScroller;

        // 各種読み込みを監視する対象
        List<ILoadData> needLoadList = new List<ILoadData>(); 

        UIPresenter(
            ThrowButtonView tButtonViewIn,
            BombButtonView bButtonViewIn,
            PlayerLife pLifeIn,
            HouseLife hLifeIn,
            BossLife bLifeIn,
            SpeedInfo sInfoIn,
            HeightInfo hInfoIn,
            Combo comboIn,
            PlayTime timeIn,
            Score scoreIn,
            Money moneyIn,
            LoadingView loadingViewIn,
            GameController gameControllerIn,
            Player playerIn,
            House houseIn,
            EnemyGenerator enemyGeneratorIn,
            BGScroller bgScrollerIn
            )
        {
            // それぞれ代入
            tButtonView = tButtonViewIn;
            bButtonView = bButtonViewIn;
            pLife = pLifeIn;
            hLife = hLifeIn;
            bLife = bLifeIn;
            sInfo = sInfoIn;
            hInfo = hInfoIn;
            combo = comboIn;
            time = timeIn;
            score = scoreIn;
            money = moneyIn;
            loadingView = loadingViewIn;

            gameController = gameControllerIn;
            player = playerIn;
            house = houseIn;
            enemyGenerator = enemyGeneratorIn;
            bgScroller = bgScrollerIn;

            // 各種情報の監視と表示更新はここでは行わない

            // 読み込み待ちの監視登録
            CheckLoadData();

        }

        // 設定・情報読み込みの監視設定
        void CheckLoadData()
        {
            // 対象を決め打ちで入れていく
            needLoadList.Add(gameController);
            needLoadList.Add(enemyGenerator);
            needLoadList.Add(bgScroller);
            needLoadList.Add(house);

            // 監視設定
            foreach(ILoadData target in needLoadList)
            {
                // 読み込み完了時の関数を登録
                target.OnLoadCompleteProperty.Subscribe(_ => OnSettingInfoLoaded());
            }

            return;
        }

        // ひとつの監視対象の各種設定・情報読み込み完了処理
        void OnSettingInfoLoaded()
        {
            bool complete = true;
            // 監視対象がすべて読み込み完了しているか確認する
            foreach(ILoadData target in needLoadList)
            {
                // ひとつでも終わっていなかったら完了とはならない
                if (false == target.LoadCompleted()) complete = false;
            }

            // すべて読み込み完了していたら完了時の処理へ
            if (true == complete) OnSettingInfoLoadCompleted();

            return;
        }

        // 各種設定・情報読み込みがすべて完了した際の処理
        void OnSettingInfoLoadCompleted()
        {
            /******各種設定反映*************************************************************************************/
            // GameController経由で取得・設定
            // ・上昇速度倍率上限下限
            // ・高度上限下限
            // ・ハイスコア
            sInfo.SetMinMagnification(gameController.SpeedMinMagnification);
            sInfo.SetMaxMagnification(gameController.SpeedMaxMagnification);
            hInfo.SetStartHeight(gameController.HeightMin);
            hInfo.SetGoalHeight(gameController.HeightMax);
            score.SetHiScore(gameController.HiScore);
            
            /******各種監視*************************************************************************************/
            // GameController監視(上昇速度倍率, 現在高度, コンボ数, ...)
            gameController.SpeedMagReactiveProperty.DistinctUntilChanged().Subscribe(x => sInfo.UpdateCurrentSpeed(x));
            gameController.HeightReactiveProperty.DistinctUntilChanged().Subscribe(x => hInfo.UpdateCurrentHeight((int)x));
            gameController.ComboReactiveProperty.DistinctUntilChanged().Subscribe(x => combo.UpdateCombo(x));
            gameController.PlayTimeReactiveProperty.DistinctUntilChanged().Subscribe(x => time.UpdatePlayTime(x));
            gameController.ScoreReactiveProperty.DistinctUntilChanged().Subscribe(x => score.UpdateScore(x));
            gameController.MoneyReactiveProperty.DistinctUntilChanged().Subscribe(x => money.UpdateMoney(x));

            // Player監視(ライフ・ボム数)
            player.LifeReactiveProperty.Subscribe(life => pLife.UpdateLife(life));
            player.BombReactiveProperty.Subscribe(stock => bButtonView.UpdateBombStock(stock));
            // TODO:魚の状態監視も必要

            // House監視(最大/現在ライフ)
            house.CurrentLifeReactiveProperty.Subscribe(life => hLife.UpdateCurrentLife(life));
            house.MaxLifeReactiveProperty.Subscribe(life => hLife.UpdateMaxLife(life));

            // LoadingView除去
            loadingView.RemoveLoadingPanel();

            return;
        }

    }
}