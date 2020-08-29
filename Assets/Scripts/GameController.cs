﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DataBase;
using Zenject;

// 各種設定値変更の方法
public enum EditMode
{
    Value_Direct,   // 直接指定(値)
    Value_Delta,    // 現在値からの変化量で指定(値)
    Rate_Direct,    // 直接指定(全体に対する割合)
    Rate_Delta      // 現在値からの変化量で指定(全体に対する割合)
}

public class GameController : MonoBehaviour
{
    /***** ReactivePropertyで監視させるものたち ****************************************************/
    // IReadOnlyReactivePropertyで公開してValueは変更できないようにする
    // プレイ時間
    ReactiveProperty<float> _playTimeReactiveProperty = new ReactiveProperty<float>(default);
    public IReadOnlyReactiveProperty<float> PlayTimeReactiveProperty { get { return _playTimeReactiveProperty; } }

    // コンボ数
    ReactiveProperty<int> _comboReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> ComboReactiveProperty { get { return _comboReactiveProperty; } }

    // スコア　※シーンをまたいで引き継ぐ
    static ReactiveProperty<int> _scoreReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> ScoreReactiveProperty { get { return _scoreReactiveProperty; } }

    // 所持金
    ReactiveProperty<int> _moneyReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> MoneyReactiveProperty { get { return _moneyReactiveProperty; } }

    // 現在高度
    ReactiveProperty<int> _heightReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> HeightReactiveProperty { get { return _heightReactiveProperty; } }

    // 上昇速度倍率
    ReactiveProperty<float> _speedMagReactiveProperty = new ReactiveProperty<float>(default);
    public IReadOnlyReactiveProperty<float> SpeedMagReactiveProperty { get { return _speedMagReactiveProperty; } }

    // 読み込み完了
    ReactiveProperty<bool> _onLoadCompleteProperty = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> OnLoadCompleteProperty => _onLoadCompleteProperty;

    /***** 設定値読み込みで保持するものたち ****************************************************/
    float speedMaxMagnification = default;  // 速度倍率上限
    float speedMinMagnification = default;  // 速度倍率下限
    int heightMax = default;                // 高度上限
    int heightMin = default;                // 高度下限
    int hiScore = default;                  // ハイスコア(更新される可能性あり)
    // それぞれ参照用に公開する


    /***** その他プレー情報 ********************************************************************/
    // 現在のステージ
    static StageNumber playingStage = default;       // シーンをまたいで保持する
    public StageNumber PlayingStage => playingStage; // 参照用

    // 現在のコンティニュー回数
    static int continueTimes = default;              // シーンをまたいで保持する
    public int ContinueTimes => continueTimes;       // 参照用


    /***** MonoBehaviourイベント処理 ****************************************************/
    void Start()
    {
        // 各種設定・情報を読み込む
        LoadData();
    }

    void FixedUpdate()
    {
        // プレー時間
        _playTimeReactiveProperty.Value += Time.deltaTime;
        // Debug.Log("Fixed Updated.playTime:" + _playTimeReactiveProperty);
    }

    /***** GameController処理 ****************************************************/
    // 各種設定・情報読み込み
    void LoadData()
    {
        // ステージ構成情報取得
        StageInfo stageInfo = DataLibrarian.Instance.GetStageInfo(playingStage);
        if (null == stageInfo)
        {
            Debug.Log("GameController LoadData failed... cannot get StageInfo");
            return;
        }

        // プレーデータと各種強化テーブル取得
        UserData userData = DataLibrarian.Instance.GetUserData();
        ReinforcementTableInfo rtInfo = DataLibrarian.Instance.GetReinforcementTableInfo();

        // 各種強化レベルと対応パラメータから上昇速度範囲を設定
        speedMaxMagnification = rtInfo.GetSpeedMaxMagRange(userData.GetLevel(ReinforceTarget.SpeedMagRange));
        speedMinMagnification = rtInfo.GetSpeedMinMagRange(userData.GetLevel(ReinforceTarget.SpeedMagRange));
        // 現在設定値は固定とする
        _speedMagReactiveProperty.Value = 1.0f;

        // ステージ構成情報から現在ステージのスタートゴール高度取得
        heightMax = stageInfo.GetHeightGoal();
        heightMin = stageInfo.GetHeightStart();
        // スタート高度に設定
        _heightReactiveProperty.Value = heightMin;

        // ハイスコア設定
        hiScore = userData.GetHighScore();

    }

    // データ初期化(新規ゲーム開始時 シーンロード前に呼ぶこと！)
    static public void InitStaticData(StageNumber stage)
    {
        Debug.Log("GameController InitStaticData, stage: " + stage.ToString());

        _scoreReactiveProperty.Value = 0;
        continueTimes = 0;
        playingStage = stage;
    }

    // 速度倍率変更操作
    public void UpdateSpeedMagnification(float value, EditMode mode)
    {
        // 割合指定の場合は全体量にかけて値に変換する
        if (EditMode.Rate_Direct == mode || EditMode.Rate_Delta == mode)
        {
            value *= speedMaxMagnification - speedMinMagnification;
        }

        // 直接指定 or 変化量で指定
        if (EditMode.Value_Direct == mode || EditMode.Rate_Direct == mode)
        {
            // 範囲限定
            if (speedMaxMagnification < value) value = speedMaxMagnification;
            if (speedMinMagnification > value) value = speedMinMagnification;

            _speedMagReactiveProperty.Value = value;
        }
        else if (EditMode.Value_Delta == mode || EditMode.Rate_Delta == mode)
        {
            // 範囲限定
            if (speedMaxMagnification < _speedMagReactiveProperty.Value + value)
            {
                _speedMagReactiveProperty.Value = speedMaxMagnification;
            }
            else if (speedMinMagnification > _speedMagReactiveProperty.Value + value)
            {
                _speedMagReactiveProperty.Value = speedMinMagnification;
            }
            else
            {
                _speedMagReactiveProperty.Value += value;
            }
        }
        else
        {
            // Do nothing
        }
        return;
    }
}
