using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

namespace InputProvider
{
    public class TouchOperation : MonoBehaviour
    {
        // タッチ情報監視
        private Subject<List<Touch>> onTouchSubject = new Subject<List<Touch>>();
        public IObservable<List<Touch>> OnTouch => onTouchSubject;

        List<Touch> touchList = new List<Touch>(5);  // Capacityを5で置いておく

        void Start()
        {
            touchList.Clear();
            IObservable<Unit> updateObserbable = this.UpdateAsObservable();

            // タッチ操作を監視
            updateObserbable
                .Where(_ => 0 < Input.touchCount)  // 1点以上タッチされていること
                .Subscribe(_ =>
                {
                    // そのフレームのタッチ情報を一通り発行
                    for (int count = 0; count < Input.touchCount; count++)
                    {
                        touchList.Add(Input.GetTouch(count));
                        //Debug.Log("Add Touch index:" + count);
                    }
                    onTouchSubject.OnNext(touchList);
                    touchList.Clear();
                })
                .AddTo(this);
        }

    }
}
