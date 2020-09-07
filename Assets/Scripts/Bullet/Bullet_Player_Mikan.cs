using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBase;

namespace Bullet
{
    public class Bullet_Player_Mikan : Bullet
    {
        // static変数として威力を取得・保持しておく
        static float power = default;
        static Bullet_Player_Mikan()
        {
            // SetPower();　ここでは呼べないのでBulletGeneratorから呼ぶことにする
        }
        public static void SetPower()
        {
            // プレーデータと各種強化テーブル取得
            UserData userData = DataLibrarian.Instance.GetUserData();
            ReinforcementTableInfo rtInfo = DataLibrarian.Instance.GetReinforcementTableInfo();

            // 読み込み成否確認
            if (null == userData || null == rtInfo)
            {
                // 駄目だった
                Debug.Log("Bullet_Player_Mikan LoadData failed... use default power...");
                power = default;
            }
            else
            {
                // Shotの威力を設定
                power = rtInfo.GetShotPower(userData.GetLevel(ReinforceTarget.ShotPower));
            }
        }


        // パラメータ指定して発射 派生動作
        public override void Shot(float angle, float? moveSpeed = null, float? rotateSpeed = null, float? accel = null, float? size = null, Color? color = null)
        {
            // 基底動作で各種変数を格納
            base.Shot(angle, moveSpeed, rotateSpeed, accel, size, color);

            // 発射
            body.AddForce(moveSpeed2D, ForceMode2D.Impulse);
            // body.AddTorque(this.rotateSpeed);  回転はしない

        }

        // 毎フレームの動作
        public override void FixedUpdate()
        {
 
        }
    }
}