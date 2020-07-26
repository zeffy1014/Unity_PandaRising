using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace UI
{
    public class PlayTime : MonoBehaviour
    {
        public Text playTimeText; // 表示テキスト
        public GameController gc; // 監視対象となるGameController

        // Start is called before the first frame update
        void Start()
        {
            // 監視対象登録
            gc.PlayTimeReactiveProperty.DistinctUntilChanged().Skip(1).Subscribe(x => playTimeText.text = x.ToString("f2")).AddTo(this);
        }

        // Update is called once per frame
        void Update()
        {

        }

    }
}