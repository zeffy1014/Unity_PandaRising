using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

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

    // スコア
    ReactiveProperty<int> _scoreReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> ScoreReactiveProperty { get { return _scoreReactiveProperty; } }

    // 所持金
    ReactiveProperty<int> _moneyReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> MoneyReactiveProperty { get { return _moneyReactiveProperty; } }

    // 現在高度
    ReactiveProperty<int> _hightReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> HightReactiveProperty { get { return _hightReactiveProperty; } }

    // 上昇速度倍率
    ReactiveProperty<float> _speedMagReactiveProperty = new ReactiveProperty<float>(default);
    public IReadOnlyReactiveProperty<float> SpeedMagReactiveProperty { get { return _speedMagReactiveProperty; } }


    /***** MonoBehaviourイベント処理 ****************************************************/
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        _playTimeReactiveProperty.Value += Time.deltaTime;
        // Debug.Log("Fixed Updated.playTime:" + _playTimeReactiveProperty);
    }
}
