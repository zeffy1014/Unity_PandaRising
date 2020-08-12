using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class House : MonoBehaviour
{
    [SerializeField] int maxLife = 1000;          // 最大耐久値
    [SerializeField] int autoHealAmount = 1;      // 自然回復量
    [SerializeField] float autoHealCycle = 0.05f; // 1回の自然回復にかかる時間

    private float waitHeal;                       // 回復待ち時間

    /***** ReactivePropertyで監視させるもの ****************************************************/
    // IReadOnlyReactivePropertyで公開してValueは変更できないようにする
    // 現在ライフ
    private ReactiveProperty<int> _currentLifeReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> CurrentLifeReactiveProperty { get { return _currentLifeReactiveProperty; } }

    // 最大ライフ
    private ReactiveProperty<int> _maxLifeReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> MaxLifeReactiveProperty { get { return _maxLifeReactiveProperty; } }


    /***** MonoBehaviourイベント処理 ****************************************************/
    void Awake()
    {
        // 各種初期化
        _maxLifeReactiveProperty.Value = maxLife;
        _currentLifeReactiveProperty.Value = maxLife;
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
