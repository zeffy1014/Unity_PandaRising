using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class HouseLife : MonoBehaviour
{
    [SerializeField] Text lifeLabel;
    [SerializeField] Image frontSprite;
    [SerializeField] Image backSprite;

    private Vector4 damageColor;              // ダメージ部分の色
    private Vector4 healColor;                // 回復部分の色
    private int currentLife = default;        // 表示用 現在ライフ
    private int maxLife = default;            // 表示用 最大ライフ
    private int nowLife = -1;                 // ダメージ表現で一定速度で追従させる見た目のライフ

    void Awake()
    {
        // 初期設定
        damageColor = new Vector4(0.5f, 0, 0, 1);
        healColor = new Vector4(0.3f, 1, 0.5f, 1);

        frontSprite.fillAmount = 1.0f;
        backSprite.fillAmount = 1.0f;
        lifeLabel.text = "----/----";
    }

    void UpdateCurrentLife(int life)
    {
        currentLife = life;
        // 初回設定
        if (-1 == nowLife) nowLife = currentLife;
    }

    void UpdateMaxLife(int life)
    {
        maxLife = life;
    }

    private void Update()
    {
        // ゲージを動かす(currentLifeが正しい耐久値なのでそれに合わせて見た目を変える)
        if (nowLife != currentLife)
        {
            if (nowLife > currentLife)
            {
                // ダメージ表現: frontがcurrentの値に変化した後backがdamageColorで追従する
                nowLife -= Mathf.FloorToInt(maxLife * Time.deltaTime * 1.0f);
                if (nowLife < currentLife) nowLife = currentLife;

                backSprite.color = damageColor;
                frontSprite.fillAmount = (float)currentLife / (float)maxLife;
                backSprite.fillAmount = (float)nowLife / (float)maxLife;
            }
            else
            {
                // 回復表現: backがhealColorでcurrentの値に変化した後frontが追従する
                nowLife += Mathf.FloorToInt(maxLife * Time.deltaTime * 0.3f);
                if (nowLife > currentLife) nowLife = currentLife;

                backSprite.color = healColor;
                backSprite.fillAmount = (float)currentLife / (float)maxLife;
                frontSprite.fillAmount = (float)nowLife / (float)maxLife;

            }
            lifeLabel.text = nowLife + "/" + maxLife;
        }
    }

}
