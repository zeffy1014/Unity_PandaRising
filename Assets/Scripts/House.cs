using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DataBase;

public class House : MonoBehaviour
{
    [SerializeField] int maxLife = 1000;          // 最大耐久値
    [SerializeField] int autoHealAmount = 1;      // 自然回復量
    [SerializeField] float autoHealCycle = 0.05f; // 1回の自然回復にかかる時間

    private float waitHeal;                       // 回復待ち時間

    /***** ReactivePropertyで監視させるもの ****************************************************/
    // IReadOnlyReactivePropertyで公開してValueは変更できないようにする
    // 現在ライフ　※シーンをまたいで引き継ぐ
    private static  ReactiveProperty<int> _currentLifeReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> CurrentLifeReactiveProperty { get { return _currentLifeReactiveProperty; } }

    // 最大ライフ
    private ReactiveProperty<int> _maxLifeReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> MaxLifeReactiveProperty { get { return _maxLifeReactiveProperty; } }


    /***** MonoBehaviourイベント処理 ****************************************************/
    void Start()
    {
        // 設定読み込み
        LoadData();

        waitHeal = 0.0f;

    }

    void Update()
    {
        // 死んでなければ一定時間おきに自然回復する
        waitHeal += Time.deltaTime;
        if (autoHealCycle <= waitHeal && 0 < _currentLifeReactiveProperty.Value)
        {
            OnHeal(autoHealAmount);
            waitHeal = 0;
        }
    }

    /***** House独自処理 ****************************************************/
    // 各種設定・情報読み込み
    void LoadData()
    {
        // プレーデータと各種強化テーブル取得
        UserData userData = DataLibrarian.Instance.GetUserData();
        ReinforcementTableInfo rtInfo = DataLibrarian.Instance.GetReinforcementTableInfo();

        // 各種強化レベルと対応パラメータから最大ライフと回復量を設定
        _maxLifeReactiveProperty.Value = rtInfo.GetHouseDurability(userData.GetLevel(ReinforceTarget.HouseDurability));
        autoHealAmount = rtInfo.GetHouseHealingPower(userData.GetLevel(ReinforceTarget.HouseHealingPower));

        return;
    }

    // データ初期化(新規ゲーム開始時 シーンロード前に呼ぶこと！)
    static public void InitStaticData()
    {
        Debug.Log("House InitStaticData");

        // ちょっと冗長だけれども現在ライフを家の最大耐久値で初期化する
        UserData userData = DataLibrarian.Instance.GetUserData();
        ReinforcementTableInfo rtInfo = DataLibrarian.Instance.GetReinforcementTableInfo();
        _currentLifeReactiveProperty.Value = rtInfo.GetHouseDurability(userData.GetLevel(ReinforceTarget.HouseDurability));

        return;
    }

    // 被ダメージ処理
    public void OnDamage(int damage)
    {
        // 現在ライフを減らす(ただしゼロ未満にはしない)
        _currentLifeReactiveProperty.Value = (damage > _currentLifeReactiveProperty.Value)
            ? (0)
            : (_currentLifeReactiveProperty.Value -= damage);

    }

    // 回復処理
    public void OnHeal(int heal)
    {
        // 現在ライフを回復する(ただし最大値は超えない)
        _currentLifeReactiveProperty.Value = (_currentLifeReactiveProperty.Value + heal > _maxLifeReactiveProperty.Value)
            ? (_maxLifeReactiveProperty.Value)
            : (_currentLifeReactiveProperty.Value + heal);

    }
}
