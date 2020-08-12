using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUI
{
    public class BossLife : MonoBehaviour
    {
        [SerializeField] Image frontImage;
        [SerializeField] Image backImage;

        int maxLife = default;

        // 最初は非表示
        void Awake()
        {
            frontImage.gameObject.SetActive(false);
            backImage.gameObject.SetActive(false);
        }

        // ライフを表示する際はこちらを呼ぶ
        void ShowLife(int maxLife)
        {
            this.maxLife = maxLife;

            frontImage.fillAmount = 0.0f;
            frontImage.gameObject.SetActive(true);
            backImage.gameObject.SetActive(true);

            // 2秒かけてライフ表示
            StartCoroutine(ShowLifeAnimation(2.0f));

        }

        // ライフを指定秒数かけて表示する
        IEnumerator ShowLifeAnimation(float showTime)
        {
            float showSpan = 0.03f;  // 33fpsを意識した表示更新間隔
            int count = 0;  // ループカウンタ

            while (frontImage.fillAmount < 1.0f)
            {
                frontImage.fillAmount = (1.0f / (showTime / showSpan)) * count;
                count++;
                yield return new WaitForSeconds(showSpan);
            }
        }

        void UpdateCurrentLife(int life)
        {
            frontImage.fillAmount = (float)life / (float)maxLife;
        }
    }
}