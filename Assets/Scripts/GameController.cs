using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DataBase;
using Zenject;
using System;

// 各種設定値変更の方法
public enum EditMode
{
    Value_Direct,   // 直接指定(値)
    Value_Delta,    // 現在値からの変化量で指定(値)
    Rate_Direct,    // 直接指定(全体に対する割合)
    Rate_Delta      // 現在値からの変化量で指定(全体に対する割合)
}

// ゲーム中の状態
public enum GameState
{
    Ready,          // プレー開始前
    Playing,        // プレー中
    GameOver,       // ゲームオーバー(リトライ確認なども)
    StageClear,     // ステージクリア(リザルト表示やメニューなども)
    Pause,          // ポーズ中

    GameState_Num
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
    ReactiveProperty<float> _heightReactiveProperty = new ReactiveProperty<float>(default);
    public IReadOnlyReactiveProperty<float> HeightReactiveProperty { get { return _heightReactiveProperty; } }

    // 上昇速度倍率
    ReactiveProperty<float> _speedMagReactiveProperty = new ReactiveProperty<float>(default);
    public IReadOnlyReactiveProperty<float> SpeedMagReactiveProperty { get { return _speedMagReactiveProperty; } }

    // ゲームの状態
    ReactiveProperty<GameState> _gameStateReactiveProperty = new ReactiveProperty<GameState>(default);
    public IReadOnlyReactiveProperty<GameState> GameStateReactiveProperty { get { return _gameStateReactiveProperty; } }

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

    // デバッグ用にインスペクターからステージ指定できるようにする
    [SerializeField] bool debugStage = false;
    [SerializeField] StageNumber debugStageNumber = default;

    // 現在のコンティニュー回数
    static int continueTimes = default;              // シーンをまたいで保持する
    public int ContinueTimes => continueTimes;       // 参照用

    // 現在の上昇速度
    float risingSpeed = default;

    // コンボ持続時間関連
    [SerializeField] float defaultComboDuration = 3.0f;  // 初期値
    [SerializeField] float finalComboDuration   = 1.0f;  // 最終的にここまで短くなる
    [SerializeField] int finalCombo = 100;               // このコンボ数までの間で持続時間が短くなっていく・倍率ボーナスつく
    [SerializeField] float finalMagBonus = 10.0f;        // コンボ数による倍率ボーナス最大値
    float comboDuration = 0.0f;                          // 現在のコンボ持続時間(残り時間)


    /***** MonoBehaviourイベント処理 ****************************************************/
    void Awake()
    {
        // デバッグ設定の場合は強制的にステージ設定
        if (debugStage) InitStaticData(debugStageNumber);

    }

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

        // 初期状態
        _gameStateReactiveProperty.Value = GameState.Ready;

        // 上昇速度を設定
        risingSpeed = GetRisingSpeed(_speedMagReactiveProperty.Value);
    }

    void FixedUpdate()
    {
        if (GameState.Playing == _gameStateReactiveProperty.Value)
        {
            // プレー時間加算
            _playTimeReactiveProperty.Value += Time.deltaTime;
            // Debug.Log("Fixed Updated.playTime:" + _playTimeReactiveProperty);

            // 現在高度上昇
            _heightReactiveProperty.Value = (_heightReactiveProperty.Value + risingSpeed * Time.deltaTime > HeightMax)
                ? (HeightMax)
                : (_heightReactiveProperty.Value + risingSpeed * Time.deltaTime);

            // コンボ中は持続時間が減少
            if (comboDuration > 0.0f)
            {
                comboDuration -= Time.deltaTime;

                if (comboDuration <= 0.0f)
                {
                    // コンボが途切れた
                    _comboReactiveProperty.Value = 0;
                    comboDuration = 0.0f;
                }
            }
        }
    }

    /***** Zenject Signal受信 ****************************************************/
    // ゲーム開始操作
    public void OnGameStart(GameStartSignal signal)
    {
        // Ready状態→Playing状態への遷移トリガーとなる
        if (GameState.Ready == _gameStateReactiveProperty.Value)
        {
            Debug.Log("Game Play Start!");
            ChangeState(GameState.Playing);
        }

        return;
    }

    // Enemy撃破
    public void OnDefeatEnemy(DefeatEnemySignal signal)
    {
        // コンボ増加・持続時間設定
        _comboReactiveProperty.Value++;
        comboDuration = GetComboDuration(_comboReactiveProperty.Value);

        // 所持金とスコア(コンボによる倍率ボーナスあり)を増加させる
        _moneyReactiveProperty.Value += signal.dropMoney;
        // 倍率ボーナス計算 最大値あり
        float magBonus = (finalCombo >= _comboReactiveProperty.Value) ? (finalMagBonus) : (finalMagBonus * (_comboReactiveProperty.Value / finalCombo));
        // スコアに反映
        _scoreReactiveProperty.Value += (int)(signal.baseScore * (1.0f + magBonus));

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
        if (null == userData || null == rtInfo)
        {
            // 駄目だった
            Debug.Log("GameController LoadData failed...");
            return false;
        }

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

        // Audio読み込み
        // BGM(ステージとボス戦のみ)
        AudioController.Instance.LoadBGM(BGMList.Game_Stage, stageInfo.GetPathBackGroundMusicStage());
        AudioController.Instance.LoadBGM(BGMList.Game_Boss, stageInfo.GetPathBackGroundMusicBoss());
        // SE全体 TODO:SEやステージ以外のBGMはもっと大本となる部分で読み込む
        foreach (SEList index in Enum.GetValues(typeof(SEList)))
        {
            AudioController.Instance.LoadSE(index, DataLibrarian.Instance.GetSoundResource(index));
        }
        // 読み込み成功
        return true;

    }

    // 状態遷移
    void ChangeState(GameState newState)
    {
        // 何か遷移時にやることがあれば
        switch(newState)
        {
            case GameState.Ready:
                break;
            case GameState.Playing:
                if (GameState.Ready == _gameStateReactiveProperty.Value || GameState.GameOver == _gameStateReactiveProperty.Value)
                {
                    // 新規スタートまたはコンティニュー時にリセットするものたち TODO:このあたりは追って整理
                    _playTimeReactiveProperty.Value = 0.0f;
                }
                break;
            case GameState.GameOver:
                break;
            case GameState.Pause:
                break;
            default:
                break;
        }

        // 最後に現在の状態を変更
        Debug.Log("GameController Change State: " + _gameStateReactiveProperty.Value + "-->" + newState);
        _gameStateReactiveProperty.Value = newState;

        return;
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
            value *= SpeedMaxMagnification - SpeedMinMagnification;
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

        // 上昇速度を更新
        risingSpeed = GetRisingSpeed(_speedMagReactiveProperty.Value);

        return;
    }

    // 上昇速度(m/s)を計算
    float GetRisingSpeed(float mag)
    {
        // 速度倍率1.0の場合にスタート→ゴールにかかる時間を決め打ち…
        // Stage1: 2.0min
        // Stage2: 2.5min
        // Stage3～: 3.0min
        // Tutorial: 2.0min

        float basicTimeRequired = default;
        switch (PlayingStage)
        {
            case StageNumber.Stage1:
                basicTimeRequired = 2.0f * 60.0f;
                break;
            case StageNumber.Stage2:
                basicTimeRequired = 2.5f * 60.0f;
                break;
            case StageNumber.Stage3:
            case StageNumber.Stage4:
            case StageNumber.Stage5:
                basicTimeRequired = 3.0f * 60.0f;
                break;
            case StageNumber.Tutorial:
                basicTimeRequired = 2.0f * 60.0f;
                break;
            default:
                basicTimeRequired = 2.0f * 60.0f;
                break;
        }

        float basicSpeed = (HeightMax - HeightMin) / basicTimeRequired;

        // 倍率をかける
        return basicSpeed * mag;
    }

    // コンボ数に対する持続時間を取得
    public float GetComboDuration(int combo)
    {
        float ret = 0.0f;

        if (finalCombo <= combo)
        {
            // これ以上は短くならない
            ret = finalComboDuration;
        }
        else if (0 == combo)
        {
            // 今後数無しの場合はゼロで返す
            ret = 0.0f;
        }
        else
        {
            // 持続時間を計算する
            ret = defaultComboDuration - ((defaultComboDuration - finalComboDuration) * (combo / finalCombo));
        }

        return ret;
    }
}
