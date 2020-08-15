using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;


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
        private HightInfo hInfo;
        private Combo combo;
        private PlayTime time;
        private Score score;
        private Money money;

        // 監視対象
        private GameController gameController;
        private Player player;
        private House house;

        UIPresenter(
            ThrowButtonView tButtonViewIn,
            BombButtonView bButtonViewIn,
            PlayerLife pLifeIn,
            HouseLife hLifeIn,
            BossLife bLifeIn,
            SpeedInfo sInfoIn,
            HightInfo hInfoIn,
            Combo comboIn,
            PlayTime timeIn,
            Score scoreIn,
            Money moneyIn,
            GameController gameControllerIn,
            Player playerIn,
            House houseIn
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

            gameController = gameControllerIn;
            player = playerIn;
            house = houseIn;

            // 各種情報の監視と表示更新はここでは行わない

            // TODO:仮でここで呼ぶけれども後で消す
            OnSettingInfoLoaded();

        }

        // 各種設定・情報読み込み完了時の処理
        void OnSettingInfoLoaded()
        {
            /******各種設定反映*************************************************************************************/
            // TODO:設定読み込み完了などのSignalを受けて各種設定
            // ・上昇速度倍率上限下限
            // ・高度上限下限
            // ・ハイスコア
            // 一旦決め打ちで設定しておく
            sInfo.SetMinMagnification(0.7f);
            sInfo.SetMaxMagnification(1.5f);
            hInfo.SetStartHight(0);
            hInfo.SetGoalHight(2000);
            score.SetHiScore(50000);
            
            /******各種監視*************************************************************************************/
            // GameController監視(上昇速度倍率, 現在高度, コンボ数, ...)
            gameController.SpeedMagReactiveProperty.DistinctUntilChanged().Subscribe(x => sInfo.UpdateCurrentSpeed(x));
            gameController.HightReactiveProperty.DistinctUntilChanged().Subscribe(x => hInfo.UpdateCurrentHight(x));
            gameController.ComboReactiveProperty.DistinctUntilChanged().Subscribe(x => combo.UpdateCombo(x));
            gameController.PlayTimeReactiveProperty.DistinctUntilChanged().Subscribe(x => time.UpdatePlayTime(x));
            gameController.ScoreReactiveProperty.DistinctUntilChanged().Subscribe(x => score.UpdateScore(x));
            gameController.MoneyReactiveProperty.DistinctUntilChanged().Subscribe(x => money.UpdateMoney(x));

            // Player監視(ライフ・ボム数)
            player.LifeReactiveProperty.Skip(1).DistinctUntilChanged().Subscribe(life => pLife.UpdateLife(life));
            player.BombReactiveProperty.Skip(1).DistinctUntilChanged().Subscribe(stock => bButtonView.UpdateBombStock(stock));
            // TODO:魚の状態監視も必要

            // House監視(最大/現在ライフ)
            house.CurrentLifeReactiveProperty.Skip(1).DistinctUntilChanged().Subscribe(life => hLife.UpdateCurrentLife(life));
            house.MaxLifeReactiveProperty.Skip(1).DistinctUntilChanged().Subscribe(life => hLife.UpdateMaxLife(life));


        }

    }
}