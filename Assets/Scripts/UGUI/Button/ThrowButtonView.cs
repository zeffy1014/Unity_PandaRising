using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrowButtonView : MonoBehaviour
{
    [SerializeField] Image circleGauge;

    // ゲージ表示情報
    bool increase = default;   // 増加or減少方向
    float duration = default;  // 所要時間
    float elasped = default;   // 経過時間

    // 最初は非表示
    private void Awake()
    {
        circleGauge.gameObject.SetActive(false);
    }

    // 時間指定でゲージ表示(increaseがtrueならば増加, falseならば減少方向)
    public void ShowGauge(bool increase, float duration)
    {
        this.increase = increase;
        this.duration = duration;
        this.elasped = 0.0f;

        // 表示ON
        circleGauge.gameObject.SetActive(true);

        return;
    }

    // Update関数 ゲージ表示更新
    private void Update()
    {
        if(circleGauge.gameObject.activeSelf)
        {
            elasped += Time.deltaTime;
            float fillAmount = default;

            // 増加or減少方向で表示
            if (increase)
            {
                fillAmount = elasped / duration;

                // ゲージが満タンになったら消す
                if (1.0f <= fillAmount) circleGauge.gameObject.SetActive(false);
                else circleGauge.fillAmount = fillAmount;
            }
            else
            {
                fillAmount = (duration - elasped) / duration;

                // ゲージが空になったら消す
                if (0.0f >= fillAmount) circleGauge.gameObject.SetActive(false);
                else circleGauge.fillAmount = fillAmount;
            }

        }
    }
}
