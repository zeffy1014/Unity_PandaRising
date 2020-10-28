using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DataBase;

public class House : MonoBehaviour, ILoadData
{
    [SerializeField] int maxLife = 1000;          // 最大耐久値
    [SerializeField] int autoHealAmount = 1;      // 自然回復量
    [SerializeField] float autoHealCycle = 0.05f; // 1回の自然回復にかかる時間

    private float waitHeal;                       // 回復待ち時間

    // 演出
    [SerializeField] GameObject damageEffect;

    // 被弾時処理用
    public bool BeingDamaged { get; private set; } = false;     // 被弾状態かどうか
    [SerializeField] float returnInterval = 3.0f;               // 被弾状態から戻るまでの時間(sec)    
    float returnWait = default;                                 // 被弾状態の経過時間(sec)
    float blinkSpan = 0.1f;                                     // 点滅周期(sec)

    /***** ReactivePropertyで監視させるもの ****************************************************/
    // IReadOnlyReactivePropertyで公開してValueは変更できないようにする
    // 現在ライフ
    private ReactiveProperty<int> _currentLifeReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> CurrentLifeReactiveProperty { get { return _currentLifeReactiveProperty; } }

    // 最大ライフ
    private ReactiveProperty<int> _maxLifeReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> MaxLifeReactiveProperty { get { return _maxLifeReactiveProperty; } }

    /***** 読み込み完了監視 **********************************************************************/
    ReactiveProperty<bool> _onLoadCompleteProperty = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> OnLoadCompleteProperty => _onLoadCompleteProperty;
    public bool LoadCompleted() { return _onLoadCompleteProperty.Value; }


    /***** MonoBehaviourイベント処理 ****************************************************/
    void Start()
    {
        // 設定読み込み
        bool loadResult = LoadData();
        if (true == loadResult)
        {
            // 読み込み完了したらフラグを立てる
            _onLoadCompleteProperty.Value = true;
        }
        else
        {
            // TODO:読み込み失敗したらエラー通知してメインメニューに戻る？
            Debug.Log("House load data failed...");
        }

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
    bool LoadData()
    {
        // プレーデータと各種強化テーブル取得
        UserData userData = DataLibrarian.Instance.GetUserData();
        ReinforcementTableInfo rtInfo = DataLibrarian.Instance.GetReinforcementTableInfo();

        // 読み込み成否確認
        if (null == userData || null == rtInfo)
        {
            // 駄目だった
            Debug.Log("House LoadData failed...");
            return false;
        }

        // 各種強化レベルと対応パラメータから最大ライフと回復量を設定 ライフは最大回復
        _maxLifeReactiveProperty.Value = rtInfo.GetHouseDurability(userData.GetLevel(ReinforceTarget.HouseDurability));
        _currentLifeReactiveProperty.Value = _maxLifeReactiveProperty.Value;
        autoHealAmount = rtInfo.GetHouseHealingPower(userData.GetLevel(ReinforceTarget.HouseHealingPower));

        return true;
    }

    // 被ダメージ処理
    public void OnDamage(int damage, Vector2 pos)
    {
        // 点滅無敵でなければ被弾
        if (!BeingDamaged)
        {
            // 現在ライフを減らす(ただしゼロ未満にはしない)
            _currentLifeReactiveProperty.Value = (damage > _currentLifeReactiveProperty.Value)
            ? (0)
            : (_currentLifeReactiveProperty.Value - damage);

            if (0 == _currentLifeReactiveProperty.Value)
            {
                // TODO:ライフゼロの処理 Signal発行など
            }
            else
            {
                // TODO:被弾のSignal発行(コンボ切れなどに使う)

                // しばらく点滅無敵状態
                BeingDamaged = true;
                StartCoroutine(DamageBlink());
            }
        }

        // 被弾位置にエフェクト これは無敵状態でも見た目の関係で出しておく
        Instantiate(damageEffect, pos, Quaternion.identity);

    }

    // 回復処理
    public void OnHeal(int heal)
    {
        // 現在ライフを回復する(ただし最大値は超えない)
        _currentLifeReactiveProperty.Value = (_currentLifeReactiveProperty.Value + heal > _maxLifeReactiveProperty.Value)
            ? (_maxLifeReactiveProperty.Value)
            : (_currentLifeReactiveProperty.Value + heal);

    }

    // 被弾時の点滅処理
    IEnumerator DamageBlink()
    {
        returnWait = 0.0f;

        // 一定期間処理を続ける
        while (BeingDamaged)
        {
            yield return null;

            returnWait += Time.deltaTime;
            Color color = this.gameObject.GetComponent<SpriteRenderer>().material.color;
            // 経過時間に2*PIをかけると1秒で1回点滅する→更に点滅周期で割る
            color.a = Mathf.Sin((returnWait * (2 * Mathf.PI)) / blinkSpan) * 0.5f + 0.5f;
            this.gameObject.GetComponent<SpriteRenderer>().material.color = color;

            if (returnInterval < returnWait)
            {
                // 無敵時間終了 Alphaも通常に戻る
                BeingDamaged = false;
                color.a = 1.0f;
                this.gameObject.GetComponent<SpriteRenderer>().material.color = color;
            }
        }
    }
}
