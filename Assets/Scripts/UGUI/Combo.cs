using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUI
{
    public class Combo : MonoBehaviour
    {
        [SerializeField] Text comboText;            // 表示テキスト
        [SerializeField] int showThrethold = 10;   // このコンボ数以上なら表示

        // 最初は非表示
        void Awake()
        {
            comboText.gameObject.SetActive(false);
            return;
        }

        // コンボ数表示更新
        public void UpdateCombo(int combo)
        {
            if (showThrethold < combo)
            {
                if (!comboText.gameObject.activeSelf) comboText.gameObject.SetActive(true);
                comboText.text = combo.ToString();
            }
            return;
        }

    }
}
