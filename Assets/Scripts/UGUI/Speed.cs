using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speed : MonoBehaviour
{
    [SerializeField] float minMagnification = 0.5f;         // 上昇速度倍率 最小値
    [SerializeField] float maxMagnification = 2.0f;         // 上昇速度倍率 最大値

    [SerializeField] Text currentSpeedText;
    [SerializeField] Image speedNeedle;

    // 現在上昇速度倍率更新
    public void UpdateCurrentSpeed(int mag)
    {
        // そのまま文字表示
        currentSpeedText.text = "x" + mag.ToString("f1");

        // 最小～最大倍率の間で針を回転 80deg ～ -80deg
        float minMagDeg = 80.0f;
        float maxMagDeg = -80.0f;
        float angle = minMagDeg - ((minMagDeg - maxMagDeg) * (mag / (maxMagnification - minMagnification)));
        speedNeedle.transform.Rotate(0.0f, 0.0f, angle);

        return;
    }

    // 最小倍率設定
    public void SetMinMagnification(float mag)
    {
        minMagnification = mag;
        return;
    }

    // 最大倍率設定
    public void SetMaxMagnification(float mag)
    {
        maxMagnification = mag;
        return;
    }
}
