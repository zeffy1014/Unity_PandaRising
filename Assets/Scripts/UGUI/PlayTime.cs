using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UGUI
{
    public class PlayTime : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI playTimeText; // 表示テキスト

        // 表示用時間更新
        public void UpdatePlayTime(float time)
        {
            playTimeText.text = time.ToString("f2");
            return;
        }

    }
}