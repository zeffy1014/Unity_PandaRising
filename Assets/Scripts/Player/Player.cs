using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Player
{
    public class Player : MonoBehaviour
    {
        // 固定値
        const float ANGLE_GAP = 90.0f;      // デフォルトで上(90.0deg)を向くのでそこを原点として処理するためのギャップ

        // 移動範囲制限のための範囲設定
        [SerializeField] GameArea gameArea = default; // 画面範囲の元となるmask画像
        [SerializeField] float borderRatioV = 0.95f;  // 画面端に対する調整率(水平)
        [SerializeField] float borderRatioH = 0.95f;  // 画面端に対する調整率(垂直)
        Rect borderRect = new Rect();                 // 移動範囲用の矩形

        // 入力操作受け付け
        private IInputProvider input;
        [SerializeField] float moveSense = 1.0f;     // 移動量に対する実際の移動距離調整用感度
        public InputDAL.MouseInputProvider test;         // テスト用に手動で設定できるようにする

        // ライフ ReactivePropertyで監視できるようにする
        [SerializeField] int defaultLife = 3;
        private ReactiveProperty<int> _lifeReactiveProperty = new ReactiveProperty<int>(default);
        public IReadOnlyReactiveProperty<int> lifeReactiveProperty { get { return _lifeReactiveProperty; } }

        [SerializeField] Text posInfo = default;


        /***** MonoBehaviourイベント処理 ****************************************************/
        void Start()
        {
            // 初期化
            _lifeReactiveProperty.Value = defaultLife;

            // TODO:正式な入力方法設定はDIで
            input = test;

            // 移動範囲設定
            SetMoveArea();

        }

        void Update()
        {
            // 操作入力を受け取ってもろもろ動作する
            InputAction();

        }

        /***** Playe個別処理 ****************************************************/
        // 入力方法設定
        /*
        public void SetInputMethod(IInputProvider method)
        {
            input = method;
        }
        */

        // 移動範囲設定
        void SetMoveArea()
        {
            // GameAreaを一定範囲狭めたものを移動範囲とする
            borderRect = gameArea.GetGameAreaRect();

            float borderShrinkV = (borderRect.xMax - borderRect.xMin) * (1.0f - borderRatioV);
            float borderShrinkH = (borderRect.yMax - borderRect.yMin) * (1.0f - borderRatioH);

            borderRect.xMin += borderShrinkV;
            borderRect.xMax -= borderShrinkV;
            borderRect.yMin += borderShrinkH;
            borderRect.yMax -= borderShrinkH;

            /*
            Debug.Log("left-bottom:(" + borderRect.xMin.ToString("f1") + ", " + borderRect.yMin.ToString("f1") + "), " + 
                      "right-top:("   + borderRect.xMax.ToString("f1") + ", " + borderRect.yMax.ToString("f1") + ")");
             */

            return;
        }

        // Input内容に応じて動作する
        void InputAction()
        {
            if (null == input) return;

            // 移動
            Vector2 moveSpeed = input.GetMoveSpeed();
            if (Vector2.zero != moveSpeed)
            {
                // 移動量取得
                // 取得した移動速度に感度とTime.deltaTimeをかけ合わせて自機移動量を出す
                // 使用端末の画面の大きさによらず、一定距離動かしたらゲーム画面上で一定割合自機が動くことを想定
                Vector2 dist = moveSpeed * moveSense * Time.deltaTime;
                Vector2 newPos = (Vector2)transform.position + dist;

                // 画面から出ない範囲で自機移動
                if (borderRect.xMax < newPos.x) newPos.x = borderRect.xMax;
                if (borderRect.xMin > newPos.x) newPos.x = borderRect.xMin;
                if (borderRect.yMax < newPos.y) newPos.y = borderRect.yMax;
                if (borderRect.yMin > newPos.y) newPos.y = borderRect.yMin;

                transform.position = newPos;

                // Debug用位置情報表示
                //Debug.Log("move dist:" + dist + ", new pos: " + newPos);
                DispPosInfo(moveSpeed, newPos);
            }

            // ショット操作
            if (input.GetShot())
            {
                Debug.Log("Shot!!");
            }

            // 投げ操作
            float throwAngle = default;
            if (input.GetThrow(ref throwAngle))
            {
                Debug.Log("Throw!! " + throwAngle + " deg");
            }

            return;
        }

        // Debug用位置情報表示
        void DispPosInfo(Vector2 inputSpeed, Vector2 pos)
        {
            // 文字列生成
            posInfo.text = "Speed:" + inputSpeed.x.ToString("f1") + ", " + inputSpeed.y.ToString("f1") + "\nPos:" + pos.x.ToString("f1") + ", " + pos.y.ToString("f1");

            // 文字列移動
            float sideOffset = 0.0f;  // 横方向にずらす距離
            float heightOffset = 50.0f;  // 縦方向にずらす距離
            Vector2 dispPos = RectTransformUtility.WorldToScreenPoint(Camera.main, pos) + new Vector2(sideOffset, heightOffset);

            // 画面からは出さない範囲で調整
            if (Screen.width < dispPos.x + posInfo.GetComponent<RectTransform>().rect.width / 2) dispPos.x = Screen.width - posInfo.GetComponent<RectTransform>().rect.width / 2;
            if (0.0f > dispPos.x - posInfo.GetComponent<RectTransform>().rect.width / 2) dispPos.x = posInfo.GetComponent<RectTransform>().rect.width / 2;
            if (Screen.height < dispPos.y + posInfo.GetComponent<RectTransform>().rect.height / 2) dispPos.y = Screen.height - posInfo.GetComponent<RectTransform>().rect.height / 2;
            if (0.0f > dispPos.y - posInfo.GetComponent<RectTransform>().rect.height / 2) dispPos.y = posInfo.GetComponent<RectTransform>().rect.height / 2;

            posInfo.transform.position = dispPos;
        }
    }

}