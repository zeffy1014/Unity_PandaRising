using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUI
{
    public class Hight : MonoBehaviour
    {
        [SerializeField] int startHight = default;  // スタート地点の高度 ステージで固定
        [SerializeField] int goalHight = default;   // ゴール地点の高度 ステージで固定

        [SerializeField] float dispPosMin = default;  // 現在高度表示枠 どこから
        [SerializeField] float dispPosMax = default;  // 現在高度表示枠 どこまでの高さで表示するか

        [SerializeField] Text startHightText;
        [SerializeField] Text goalHightText;
        [SerializeField] Text currentHightText;
        [SerializeField] RectTransform currentHightDisp;

        // 現在高度更新
        public void UpdateCurrentHight(int hight)
        {
            // 表示テキストを更新する
            currentHightText.text = (hight.ToString() + "m");

            // 表示位置を更新する スタートゴール間に対する割合から表示位置決定
            float progress = hight / (goalHight - startHight);
            float dispPos = dispPosMin + (progress * (dispPosMax - dispPosMin));

            Vector3 newDispPos = new Vector3(currentHightDisp.position.x, dispPos, currentHightDisp.position.z);
            currentHightDisp.position = newDispPos;

            return;
        }

        // スタート地点高度表示設定
        public void SetStartHight(int hight)
        {
            startHight = hight;
            startHightText.text = (hight.ToString() + "m");
            return;
        }

        // ゴール地点高度表示設定
        public void SetGoalHight(int hight)
        {
            goalHight = hight;
            goalHightText.text = (hight.ToString() + "m");
            return;
        }
    }
}