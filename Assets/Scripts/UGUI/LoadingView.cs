using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coffee.UIEffects;

public class LoadingView : MonoBehaviour
{
    [SerializeField] float removeDuration = 1.0f;  // 画面消去にかける時間
    [SerializeField] float initialFactor = 1.0f;   // effect factor初期値
    float elapsedTime = 0.0f;                      // 画面消去経過時間

    UITransitionEffect effect;
    bool removing = false;                         // 画面消去中かどうか
    
    void Awake()
    {
        // UI Trasition Effectコンポーネント取得
        effect = this.GetComponent<UITransitionEffect>();

        // 初期値を入れる
        effect.effectFactor = initialFactor;
        
    }

    void Update()
    {
        // 消去中であれば逐次factorをいじっていく
        if (removing)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime < removeDuration)
            {
                // 進行度は経過時間/画面消去にかける時間 初期値も考慮
                effect.effectFactor = initialFactor * (1.0f - (elapsedTime / removeDuration));
            }
            else
            {
                // 消去完了
                effect.effectFactor = 0.0f;
                removing = false;
            }
        }
    }

    // Loading画面消去(時間指定なし)
    public void RemoveLoadingPanel()
    {
        // 消去中状態にして経過時間をクリア あとはUpdateにて
        removing = true;
        elapsedTime = 0.0f;
    }

    // Loading画面消去(時間指定)
    public void RemoveLoadingPanel(float duration)
    {
        removeDuration = duration;
        RemoveLoadingPanel();
    }

}
