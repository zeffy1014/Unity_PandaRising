using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrowButtonView : MonoBehaviour
{
    [SerializeField] Image circleGauge;

    // ボタンの色味
    Color defaultColor = new Color(1.0f, 1.0f, 1.0f);   // 初期状態
    Color throwingColor = new Color(0.5f, 0.5f, 0.5f);  // 投げている途中は薄暗く
    Color lostColor = new Color(0.5f, 0.2f, 0.2f);      // 失って復活待ちは赤暗く

    // ゲージ表示情報
    bool increase = default;   // 増加or減少方向
    float duration = default;  // 所要時間
    float elasped = default;   // 経過時間

    // 最初は非表示
    private void Awake()
    {
        circleGauge.gameObject.SetActive(false);
        GetComponent<Image>().color = defaultColor;
    }

    // 時間指定でゲージ表示(increaseがtrueならば増加, falseならば減少方向)
    public void ShowGauge(bool increase, float duration)
    {
        this.increase = increase;
        this.duration = duration;
        this.elasped = 0.0f;

        // 表示ON
        circleGauge.gameObject.SetActive(true);
        // ゲージ表示中はボタンの色を落とす
        if (increase) GetComponent<Image>().color = lostColor;
        else GetComponent<Image>().color = throwingColor;

        return;
    }

    // ゲージ消去
    public void HideGauge()
    {
        // ゲージを消しつつボタンの色も戻す
        circleGauge.gameObject.SetActive(false);
        GetComponent<Image>().color = defaultColor;
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
