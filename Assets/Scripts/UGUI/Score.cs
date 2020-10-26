using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Score : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI hiScoreText;

    [SerializeField] int hiScore = default;

    // スコア表示更新
    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score.ToString();

        // ハイスコア更新していたらこちらも追従
        if (hiScore < score) SetHiScore(score);

        return;
    }

    // ハイスコア設定
    public void SetHiScore(int hiScore)
    {
        this.hiScore = hiScore;
        hiScoreText.text = "Hi-Score: " + this.hiScore.ToString();
        return;
    }

}
