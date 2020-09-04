using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using System;
using Zenject;
using UGUI;

namespace InputProvider
{
    public class TouchInputProvider : IInputProvider
    {
        // Shot操作監視
        private Subject<Unit> onShotSubject = new Subject<Unit>();
        public IObservable<Unit> OnShot => onShotSubject;

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
        Touch moveTouch;    // 移動用として監視するタッチ情報

        // Suscribe先
        TouchOperation touch = default;
        OperateButton shotButton = default;
        OperateButton throwButton = default;
        OperateButton bombButton = default;
        OperateButton menuButton = default;
        SlideArea slideArea = default;

        TouchInputProvider(
            TouchOperation touch,
            // IDは各ボタンのZenjectBindingコンポーネントにて指定 この場合はenum使えない？
            [Inject(Id = "Shot")] OperateButton shotButton,
            [Inject(Id = "Throw")] OperateButton throwButton,
            [Inject(Id = "Bomb")] OperateButton bombButton,
            [Inject(Id = "Menu")] OperateButton menuButton,
            SlideArea slide)
        {
            this.touch = touch;
            this.shotButton = shotButton;
            this.throwButton = throwButton;
            this.bombButton = bombButton;
            this.menuButton = menuButton;
            this.slideArea = slide;

            // ShotはuGUIボタン操作 押しっぱなし中操作を発行
            this.shotButton.OnButton
                .Where(info => true == info.Down)
                .Subscribe(push => onShotSubject.OnNext(Unit.Default));

            // ThrowはuGUIボタン操作 押され→離されを検出して操作を発行
            this.throwButton.OnButton
                .DistinctUntilChanged(info => info.Down)
                .Subscribe(info =>
                {
                    if(info.Down)  // 押された初期位置を保持
                    {
                        if (Vector2.zero == throwBegin) throwBegin = info.Position;
                    }
                    else  // 離されたら押され位置との角度で投げ操作を発行
                    {
                        if (Vector2.zero != throwBegin)
                        {
                            onThrowSubject.OnNext(ThrowAngle(throwBegin, info.Position));
                            throwBegin = Vector2.zero;  // 初期化
                        }
                    }
                });

            // BombはuGUIボタン操作 離されたのがボタン上だったら操作を発行
            this.bombButton.OnButton
                .DistinctUntilChanged(info => info.Down)
                .Where(info => false == info.Down)
                .Subscribe(info =>
                {
                    // 離され位置がBombボタン上かどうか
                    if (true == CheckOnObject(info.Position, bombButton.gameObject)) onBombSubject.OnNext(Unit.Default);
                });

            // MenuはuGUIボタン操作 離されたのがボタン上だったら操作を発行
            this.menuButton.OnButton
                .DistinctUntilChanged(info => info.Down)
                .Where(info => false == info.Down)
                .Subscribe(info =>
                {
                    // 離され位置がMenuボタン上かどうか
                    if (true == CheckOnObject(info.Position, menuButton.gameObject)) onMenuSubject.OnNext(Unit.Default);
                });

            // Speed変更操作 Drag情報(Vector2)をボタンサイズに対する水平方向の移動割合として発行
            this.slideArea.OnSlide
                .Subscribe(delta =>
                {
                    Rect area = slideArea.GetComponent<RectTransform>().rect;
                    onSpeedEditSubject.OnNext(delta.x / area.width);
                });

            // 移動はタッチ操作
            // 初期化
            moveTouch.position = Vector2.zero;
            moveTouch.phase = TouchPhase.Canceled;
            moveTouch.fingerId = -1;

            this.touch.OnTouch
                //.DistinctUntilChanged()   // 情報が変わったときのみ(使えるか？)
                .Skip(1)
                .Subscribe(touchInfo =>
                {
                    MoveInfo moveInfo = default;
                    if (CheckMoveTouch(touchInfo, ref moveInfo)) onMovePlayerSubject.OnNext(moveInfo);
                });
        }

        bool CheckMoveTouch(List<Touch> touchInfo, ref MoveInfo moveInfo)
        {
            // uGUIに当たっていない位置でPhase:Beganの指があったら移動用としてfingerIdとタッチ情報を保持
            // (同時にBeganとなる指があった場合はindex若い方を優先)
            // 以降そのfingerIdのタッチを移動用タッチとする
            //
            // 新たなBeganの指があった場合
            //   移動用タッチの指が継続していたら無視
            //   移動用タッチの指が離れていたら(Endもしくはタッチ無し)新たなBeganのタッチを移動用として保持
            // 移動用タッチがすでにある場合はそれを確認してMoveInfoで情報保存しtrue返却
            //
            // 移動情報がない場合はfalse返却

            bool ret = false;

            // 移動用タッチがすでにある場合はそれを確認する
            if (-1 != moveTouch.fingerId)
            {
                for (int i = 0; i < touchInfo.Count; i++)
                {
                    Touch touch = touchInfo[i];
                    if (moveTouch.fingerId == touch.fingerId)
                    {
                        switch (touch.phase)
                        {
                            case TouchPhase.Moved:
                            case TouchPhase.Stationary:
                                // 移動用タッチがタッチされたまま→MoveInfo情報保存
                                if (0.0f == touch.deltaTime) return false;  // 計算不能な場合は抜ける

                                Vector2 speed = new Vector2(touch.deltaPosition.x / touch.deltaTime, touch.deltaPosition.y / touch.deltaTime);
                                Vector2 position = touch.position;
                                MoveInfo newInfo = new MoveInfo(speed, position);
                                moveInfo = newInfo;

                                // 移動用タッチ情報更新して終了
                                moveTouch = touch;
                                ret = true;
                                return ret;

                            case TouchPhase.Began:
                            case TouchPhase.Ended:
                            case TouchPhase.Canceled:
                            default:
                                // 移動用タッチ情報をクリアして先に進む
                                // Debug.Log("Finger for move has released.");
                                moveTouch.fingerId = -1;
                                break;
                        }
                        break;
                    }
                }
            }

            // ここからは新たな移動用タッチ情報があるか取得する ここではMoveInfoは無いので戻りはfalse
            for (int i = 0; i < touchInfo.Count; i++)
            {
                Touch touch = touchInfo[i];
                if (TouchPhase.Began == touch.phase)
                {
                    // タッチ開始だったらuGUIに当たっていないかチェック
                    List<RaycastResult> raycastResults = GetRayCastResults(touch.position);
                    if (0 < raycastResults.Count)
                    {
                        // 当たっていたら無効
                        // Debug.Log("Touch has began, but out of range... index:" + i);
                    }
                    else { 
                        // 当たっていなかったらそれが新たな移動用タッチ
                        Debug.Log("Here comes a new finger for move! Index:" + i + ", FingerID:" + touch.fingerId);
                        moveTouch = touch;
                    }
                }
            }

            return ret;

        }

        // ボタン操作開始終了位置からフリックの方向を出す(angleは角度(0.0f-360.f deg), -1.0fは方角指定なしとする)
        private float ThrowAngle(Vector2 startPos, Vector2 endPos)
        {
            float angle = default;

            if (Vector2.zero != startPos)
            {
                // 開始→終了の位置で方向を定める
                float diffX = endPos.x - startPos.x;
                float diffY = endPos.y - startPos.y;
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

        // 指定位置でRayを飛ばした結果を取得
        List<RaycastResult> GetRayCastResults(Vector2 position)
        {
            PointerEventData data = new PointerEventData(EventSystem.current);
            List<RaycastResult> raycastResults = new List<RaycastResult>();

            data.position = position;
            EventSystem.current.RaycastAll(data, raycastResults);

            return raycastResults;
        }

        // 指定位置した位置が任意のGameObject上か確認
        bool CheckOnObject(Vector2 position, GameObject target)
        {
            bool ret = false;

            List<RaycastResult> raycastResults = GetRayCastResults(position);
            foreach (RaycastResult result in raycastResults)
            {
                if (target == result.gameObject)
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }
    }
}