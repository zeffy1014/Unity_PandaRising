using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using System;

namespace UI
{
    public enum ButtonType
    {
        Shot,      // 通常ショット操作ボタン
        Throw,     // 投げ操作ボタン
        Bomb,      // ボム操作ボタン
        Menu       // メニュー操作ボタン
    }

    // ボタンの押され・離されと位置情報
    public class PressInfo
    {
        public PressInfo(bool down, Vector2 position)
        {
            Down = down;
            Position = position;
        }

        public bool Down { get; set; }
        public Vector2 Position { get; set; }
    }

    public class OperateButton : MonoBehaviour
    {
        [SerializeField] ButtonType type = default;

        // 監視用 押され状態か離され状態かのbool値を持つ
        private Subject<PressInfo> onButtonSubject = new Subject<PressInfo>();
        public IObservable<PressInfo> OnButton => onButtonSubject;

        private PressInfo pressInfo = new PressInfo(false, Vector2.zero);

        [SerializeField] Sprite defaultImage = null;
        [SerializeField] Sprite pushImage = null;

        void Start()
        {
            // EventTriggerを追加してボタン押され/離され(PointerDown/PointerUp)を拾う
            var eventTrigger = this.gameObject.AddComponent<ObservableEventTrigger>();
            eventTrigger.OnPointerDownAsObservable()
                .Subscribe(data =>
                {
                    Debug.Log("Button Down type:" + type);
                    pressInfo.Down = true;
                    pressInfo.Position = data.position;
                    PushButton(true);
                })
            .AddTo(this);
            eventTrigger.OnPointerUpAsObservable()
                .Subscribe(data =>
                {
                    Debug.Log("Button Up type:" + type);
                    pressInfo.Down = false;
                    pressInfo.Position = data.position;
                    PushButton(false);
                })
                .AddTo(this);

            // Updateタイミングで現在のボタン押され状況を発行する
            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    onButtonSubject.OnNext(pressInfo);
                })
                .AddTo(this);
        }

        void PushButton(bool startPush)
        {
            // 表示画像変更
            if (startPush)
            {
                // 押され画像があればそれにする
                if (null != pushImage) this.GetComponent<Image>().sprite = pushImage;
            }
            else
            {
                // 離され(デフォルト)画像があればそれにする
                if (null != defaultImage) this.GetComponent<Image>().sprite = defaultImage;
            }
        }
    }
}