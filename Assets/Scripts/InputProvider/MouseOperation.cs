using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

namespace InputProvider
{
    public class MouseOperation : MonoBehaviour
    {
        // 左クリック監視
        private Subject<Unit> onLeftClickSubject = new Subject<Unit>();
        public IObservable<Unit> OnLeftClick => onLeftClickSubject;

        // 左クリック離され監視
        private Subject<Unit> onLeftClickUpSubject = new Subject<Unit>();
        public IObservable<Unit> OnLeftClickUp => onLeftClickUpSubject;

        // 右クリック押され監視
        private Subject<Vector2> onRightClickDownSubject = new Subject<Vector2>();
        public IObservable<Vector2> OnRightClickDown => onRightClickDownSubject;

        // 右クリック離され監視
        private Subject<Vector2> onRightClickUpSubject = new Subject<Vector2>();
        public IObservable<Vector2> OnRightClickUp => onRightClickUpSubject;

        // 中クリック押され監視
        private Subject<Unit> onWheelClickDownSubject = new Subject<Unit>();
        public IObservable<Unit> OnWheelClickDown => onWheelClickDownSubject;

        // マウス移動速度・位置監視
        private Subject<MoveInfo> onMoveSubject = new Subject<MoveInfo>();
        public IObservable<MoveInfo> OnMove => onMoveSubject;

        void Start()
        {
            IObservable<Unit> updateObserbable = this.UpdateAsObservable();

            // マウス操作を監視
            updateObserbable
                .Subscribe(_ =>
                {
                    if (Input.GetMouseButton(0))
                    {
                        onLeftClickSubject.OnNext(Unit.Default);
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        onLeftClickUpSubject.OnNext(Unit.Default);
                    }
                    if (Input.GetMouseButtonDown(1))
                    {
                        onRightClickDownSubject.OnNext(Input.mousePosition);
                    }
                    if (Input.GetMouseButtonUp(1))
                    {
                        onRightClickUpSubject.OnNext(Input.mousePosition);
                    }
                    if (Input.GetMouseButtonDown(2))
                    {
                        onWheelClickDownSubject.OnNext(Unit.Default);
                    }

                    Vector2 moveSpeed = new Vector2(
                        Input.GetAxis("Mouse X")/Time.deltaTime,
                        Input.GetAxis("Mouse Y")/Time.deltaTime);
                    if (Vector2.zero != moveSpeed)
                    {
                        onMoveSubject.OnNext(new MoveInfo(moveSpeed, Input.mousePosition));
                    }
                })
                .AddTo(this);

        }
    }
}