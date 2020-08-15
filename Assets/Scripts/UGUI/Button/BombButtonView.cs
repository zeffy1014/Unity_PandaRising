using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombButtonView : MonoBehaviour
{
    [SerializeField] Text stockText;

    // ボムのストック数更新
    public void UpdateBombStock(int stock)
    {
        stockText.text = stock.ToString();
        return;
    }

}
