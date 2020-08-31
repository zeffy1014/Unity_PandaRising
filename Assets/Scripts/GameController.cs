using System.Collections;
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

public class GameController : MonoBehaviour, ILoadData
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

    /***** 読み込み完了監視 **********************************************************************/
    ReactiveProperty<bool> _onLoadCompleteProperty = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> OnLoadCompleteProperty => _onLoadCompleteProperty;
    public bool LoadCompleted() { return _onLoadCompleteProperty.Value; }

    /***** 設定値読み込みで保持するものたち ****************************************************/
    public float SpeedMaxMagnification { get; private set; } = default;  // 速度倍率上限
    public float SpeedMinMagnification { get; private set; } = default;  // 速度倍率下限
    public int HeightMax { get; private set; } = default;                // 高度上限
    public int HeightMin { get; private set; } = default;                // 高度下限
    public int HiScore { get; private set; } = default;                  // ハイスコア(更新される可能性あり)

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
        bool loadResult = LoadData();
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
    }

    void FixedUpdate()
    {
        // プレー時間
        _playTimeReactiveProperty.Value += Time.deltaTime;
        // Debug.Log("Fixed Updated.playTime:" + _playTimeReactiveProperty);
    }


    /***** GameController処理 ****************************************************/
    // 各種設定・情報読み込み
    bool LoadData()
    {
        // ステージ構成情報取得
        StageInfo stageInfo = DataLibrarian.Instance.GetStageInfo(playingStage);
        if (null == stageInfo)
        {
            Debug.Log("GameController LoadData failed... cannot get StageInfo");
            return false;
        }

        // プレーデータと各種強化テーブル取得
        UserData userData = DataLibrarian.Instance.GetUserData();
        ReinforcementTableInfo rtInfo = DataLibrarian.Instance.GetReinforcementTableInfo();

        // 各種強化レベルと対応パラメータから上昇速度範囲を設定
        SpeedMaxMagnification = rtInfo.GetSpeedMaxMagRange(userData.GetLevel(ReinforceTarget.SpeedMagRange));
        SpeedMinMagnification = rtInfo.GetSpeedMinMagRange(userData.GetLevel(ReinforceTarget.SpeedMagRange));
        // 現在設定値は固定とする
        _speedMagReactiveProperty.Value = 1.0f;

        // ステージ構成情報から現在ステージのスタートゴール高度取得
        HeightMax = stageInfo.GetHeightGoal();
        HeightMin = stageInfo.GetHeightStart();
        // スタート高度に設定
        _heightReactiveProperty.Value = HeightMin;

        // ハイスコアと所持金設定
        HiScore = userData.GetHighScore();
        _moneyReactiveProperty.Value = userData.GetPocketMoney();

        // 読み込み成功
        return true;

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
            value *= SpeedMaxMagnification - SpeedMaxMagnification;
        }

        // 直接指定 or 変化量で指定
        if (EditMode.Value_Direct == mode || EditMode.Rate_Direct == mode)
        {
            // 範囲限定
            if (SpeedMaxMagnification < value) value = SpeedMaxMagnification;
            if (SpeedMinMagnification > value) value = SpeedMinMagnification;

            _speedMagReactiveProperty.Value = value;
        }
        else if (EditMode.Value_Delta == mode || EditMode.Rate_Delta == mode)
        {
            // 範囲限定
            if (SpeedMaxMagnification < _speedMagReactiveProperty.Value + value)
            {
                _speedMagReactiveProperty.Value = SpeedMaxMagnification;
            }
            else if (SpeedMinMagnification > _speedMagReactiveProperty.Value + value)
            {
                _speedMagReactiveProperty.Value = SpeedMinMagnification;
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
