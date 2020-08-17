using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using System;

public class SlideArea : MonoBehaviour
{
    // 監視用 スライド方向・速度
    private Subject<Vector2> onSlideSubject = new Subject<Vector2>();
    public IObservable<Vector2> OnSlide => onSlideSubject;

    // タッチ中画像変更対応
    [SerializeField] Sprite defaultImage = null;
    [SerializeField] Sprite pushImage = null;

    void Start()
    {
        // EventTriggerを追加してエリアにタッチされているかどうか拾う
        var eventTrigger = this.gameObject.AddComponent<ObservableEventTrigger>();

        eventTrigger.OnPointerDownAsObservable()
            .Subscribe(data => PushButton(true))
            .AddTo(this);
        eventTrigger.OnPointerUpAsObservable()
            .Subscribe(data => PushButton(false))
            .AddTo(this);

        // スライド情報はOnDragから取得した移動量を発行
        eventTrigger.OnDragAsObservable()
            .Subscribe(data =>
            {
                //Rect rect = this.GetComponent<RectTransform>().rect;
                //Vector2 slideRate = new Vector2(data.delta.x / rect.width, data.delta.y / rect.height);
                //onSlideSubject.OnNext(slideRate);
                onSlideSubject.OnNext(data.delta);
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
