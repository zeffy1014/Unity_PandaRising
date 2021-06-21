using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UGUI
{
    public class Combo : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI comboText;         // 表示テキスト
        [SerializeField] int showThrethold = 10;            // このコンボ数以上なら表示
        [SerializeField] Image comboGauge;                  // コンボ繋がる残り時間ゲージ

        float durationTime = 0.0f;                         // コンボが繋がる時間
        float elaspedTime = 0.0f;                           // 経過時間

        // 最初は非表示
        void Awake()
        {
            comboText.gameObject.SetActive(false);
            return;
        }

        // コンボ数表示更新
        public void UpdateCombo(int combo, float time)
        {
            if (showThrethold < combo)
            {
                if (!comboText.gameObject.activeSelf) comboText.gameObject.SetActive(true);
            }
            else
            {
                if (comboText.gameObject.activeSelf) comboText.gameObject.SetActive(false);
            }
            comboText.text = combo.ToString();

            // 残り時間設定 ゲージは満タンで表示
            durationTime = time;
            elaspedTime = 0.0f;
            comboGauge.fillAmount = 1.0f;
            if (!comboGauge.gameObject.activeSelf) comboGauge.gameObject.SetActive(true);

            return;
        }

        public void Update()
        {
            // 時間経過によるゲージ表示減少
            elaspedTime += Time.deltaTime;
            if (durationTime > elaspedTime)
            {
                // まだコンボが切れていない
                comboGauge.fillAmount = (durationTime - elaspedTime) / durationTime;
            }
            else
            {
                // コンボが途切れたのでゲージを消す
                if (comboGauge.gameObject.activeSelf) comboGauge.gameObject.SetActive(false);
                elaspedTime = 0.0f;
            }
        }
    }
}
