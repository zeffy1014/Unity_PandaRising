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
    int combo = 0;
    ReactiveProperty<int> _comboReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> ComboReactiveProperty { get { return _comboReactiveProperty; } }

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
