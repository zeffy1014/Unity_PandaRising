using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] Text hiScoreText;

    [SerializeField] int hiScore = default;

    // スコア表示更新
    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();

        // ハイスコア更新していたらこちらも追従
        if (hiScore < score) SetHiScore(score);

        return;
    }

    // ハイスコア設定
    public void SetHiScore(int hiScore)
    {
        this.hiScore = hiScore;
        hiScoreText.text = this.hiScore.ToString();
        return;
    }

}
