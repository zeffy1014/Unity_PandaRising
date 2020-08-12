using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUI
{
    public class Money : MonoBehaviour
    {
        [SerializeField] Text moneyText; // 表示テキスト

        // 表示用所持金更新
        public void UpdateMoney(int money)
        {
            moneyText.text = money.ToString();
            return;
        }

    }
}
