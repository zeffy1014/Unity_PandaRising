using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUI
{
    public class HeightInfo : MonoBehaviour
    {
        [SerializeField] int startHeight = default;  // スタート地点の高度 ステージで固定
        [SerializeField] int goalHeight = default;   // ゴール地点の高度 ステージで固定

        [SerializeField] float dispPosMin = default;  // 現在高度表示枠 どこから
        [SerializeField] float dispPosMax = default;  // 現在高度表示枠 どこまでの高さで表示するか

        [SerializeField] Text startHeightText;
        [SerializeField] Text goalHeightText;
        [SerializeField] Text currentHeightText;
        [SerializeField] RectTransform currentHeightDisp;

        // 現在高度更新
        public void UpdateCurrentHeight(int height)
        {
            // 表示テキストを更新する
            currentHeightText.text = (height.ToString() + "m");

            // 表示位置を更新する スタートゴール間に対する割合から表示位置決定
            float progress = height / (goalHeight - startHeight);
            float dispPos = dispPosMin + (progress * (dispPosMax - dispPosMin));

            Vector3 newDispPos = new Vector3(currentHeightDisp.position.x, dispPos, currentHeightDisp.position.z);
            currentHeightDisp.position = newDispPos;

            return;
        }

        // スタート地点高度表示設定
        public void SetStartHeight(int height)
        {
            startHeight = height;
            startHeightText.text = (height.ToString() + "m");
            return;
        }

        // ゴール地点高度表示設定
        public void SetGoalHeight(int height)
        {
            goalHeight = height;
            goalHeightText.text = (height.ToString() + "m");
            return;
        }
    }
}