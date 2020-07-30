using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

namespace InputProvider
{
    // マウスによる操作入力Class
    public class MouseInputProvider : MonoBehaviour, IInputProvider
    {
        Vector2 startRightClickPos = Vector2.zero;  // 右クリック位置の保持変数

        /***** IInputProvider継承メソッド ********************************************/
        // ショット操作がされているか
        public bool GetShot()
        {
            // ショット操作 = Left Click
            bool ret = Input.GetMouseButton(0);
            return ret;
        }

        // 投げ操作がされているか(angleは角度(0.0f-360.f deg), -1.0fは方角指定なしとする)
        public bool GetThrow(ref float angle)
        {
            bool ret = false;

            // 投げ操作 = Right Drag & Drop(Right ClickのみもOKとする)
            // この関数では右クリック離しを検出する
            if (Input.GetMouseButtonUp(1))
            {
                if (Vector2.zero != startRightClickPos)
                {
                    // クリック開始→終了の位置で方向を定める
                    Vector2 endRightClickPos = Input.mousePosition;
                    float diffX = endRightClickPos.x - startRightClickPos.x;
                    float diffY = endRightClickPos.y - startRightClickPos.y;
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

                    ret = true;

                    //Debug.Log("Right Click End: " + endRightClickPos);
                    //Debug.Log("  diff:(" + diffX + "," + diffY + "), angle:" + angle);

                    // 初期化
                    startRightClickPos = Vector2.zero;
                }
            }

            return ret;
        }

        // 自機移動量
        public Vector2 GetMoveSpeed()
        {
            // 移動速度 = マウスカーソルの移動量/deltaTime ※Screen座標
            Vector2 ret = Vector2.zero;

            ret.x = Input.GetAxis("Mouse X") / Time.deltaTime;
            ret.y = Input.GetAxis("Mouse Y") / Time.deltaTime;
            //Debug.Log("Mouse deltaTime: " + Time.deltaTime);

            return ret;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // マウス右クリックがあった場合は位置を保持しておく
            if (Input.GetMouseButtonDown(1))
            {
                startRightClickPos = Input.mousePosition;
                //Debug.Log("Right Click Start: " + startRightClickPos);
            }
        }
    }
}