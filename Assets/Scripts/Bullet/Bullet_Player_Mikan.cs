using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBase;
using Enemy;

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
        public override void Shot(float angle)
        {
            // 基底動作で速度設定・RigidBody2D取得など
            base.Shot(angle);

            // 発射
            body.AddForce(moveSpeed2D, ForceMode2D.Impulse);
            // body.AddTorque(this.rotateSpeed);  回転はしない

        }

        public override void OnTriggerEnter2D(Collider2D other)
        {
            // 壁に接触したら消える
            if ("Wall" == other.tag)
            {
                Destroy(this.gameObject);
            }
            // 敵に接触したらダメージを与えて消える
            if ("Enemy" == other.tag)
            {
                other.GetComponent<EnemyBase>().OnDamage(power);
                Destroy(this.gameObject);
            }

        }
    }
}