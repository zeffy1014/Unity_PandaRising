using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

namespace InputProvider
{
    public class PCInputProvider : IInputProvider
    {
        // Shot操作監視
        private Subject<bool> onShotSubject = new Subject<bool>();
        public IObservable<bool> OnShot => onShotSubject;

        // Throw操作監視
        private Vector2 throwBegin = Vector2.zero;
        private Subject<float> onThrowSubject = new Subject<float>();
        public IObservable<float> OnThrow => onThrowSubject;

        // Bomb操作監視
        private Subject<Unit> onBombSubject = new Subject<Unit>();
        public IObservable<Unit> OnBomb => onBombSubject;

        // Menu操作監視
        private Subject<Unit> onMenuSubject = new Subject<Unit>();
        public IObservable<Unit> OnMenu => onMenuSubject;

        // Speed変更操作監視
        private Subject<float> onSpeedEditSubject = new Subject<float>();
        public IObservable<float> OnSpeedEdit => onSpeedEditSubject;

        // 移動監視
        private Subject<MoveInfo> onMovePlayerSubject = new Subject<MoveInfo>();
        public IObservable<MoveInfo> OnMovePlayer => onMovePlayerSubject;

        private MouseOperation mouse;
        private KeyOperation key;

        PCInputProvider(MouseOperation mouse, KeyOperation key)
        {
            this.mouse = mouse;
            this.key = key;

            /***** MouseOperationの操作を監視 *****/
            // 左クリック(押しっぱなしあり)でShot操作発行
            this.mouse.OnLeftClick.Subscribe(_ => onShotSubject.OnNext(true));

            // 左クリック離されも監視
            this.mouse.OnLeftClickUp.Subscribe(_ => onShotSubject.OnNext(false));

            // 右クリック押し→離しで投げ操作発行
            this.mouse.OnRightClickDown.Subscribe(pos => throwBegin = pos);  // 右クリック開始位置を保持
            this.mouse.OnRightClickUp
                .Subscribe(pos => 
                {
                    // 右クリック開始→終了位置に応じた角度で投げ操作を発行
                    onThrowSubject.OnNext(ThrowAngle(throwBegin, pos));
                    throwBegin = Vector2.zero;  // 初期化
                });

            // ホイールクリックでボム操作発行
            this.mouse.OnWheelClickDown.Subscribe(_ => onBombSubject.OnNext(Unit.Default));

            // マウス移動でPlayer移動操作発行
            this.mouse.OnMove.Subscribe(info => onMovePlayerSubject.OnNext(info));

            /***** KeyOperationの操作を監視 *****/
            // スペースキー押しでMenu操作発行
            this.key.OnSpaceKey.Subscribe(_ => onMenuSubject.OnNext(Unit.Default));

            // 左右キー押しで速度変更操作発行
            this.key.OnRightKey.Subscribe(_ => onSpeedEditSubject.OnNext(0.1f));  // 固定で0.1増加
            this.key.OnLeftKey.Subscribe(_ => onSpeedEditSubject.OnNext(-0.1f));  // 固定で0.1減少
        }

        // クリック開始終了位置からドラッグの方向を出す(angleは角度(0.0f-360.f deg), -1.0fは方角指定なしとする)
        private float ThrowAngle(Vector2 startPos, Vector2 endPos)
        {
            float angle = default;

            if (Vector2.zero != startPos)
            {
                // クリック開始→終了の位置で方向を定める
                Vector2 endRightClickPos = Input.mousePosition;
                float diffX = endRightClickPos.x - startPos.x;
                float diffY = endRightClickPos.y - startPos.y;
                if (diffX == 0.0f && diffY == 0.0f)
                {
                    // 角度指定なし
                    angle = -1.0f;
                }
                else
                {
                    // 0.0f-360.0fの間で角度を指定する
                    angle = Mathf.Atan2(diffY, diffX) * Mathf.Rad2Deg;
                    if (0.0f > angle) angle += 360.0f;
                }

                //Debug.Log("Right Click End: " + endRightClickPos);
                //Debug.Log("  diff:(" + diffX + "," + diffY + "), angle:" + angle);

            }
            else
            {
                // 角度指定なし
                angle = -1.0f;
            }

            return angle;
        }



        public bool GetShot() { return true; }
        public bool GetThrow(ref float angle) { return true; }
        public Vector2 GetMoveSpeed() { return Vector2.zero; }
    }
}