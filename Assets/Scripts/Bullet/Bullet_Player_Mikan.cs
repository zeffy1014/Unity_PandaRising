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
            // 障害物に接触したら消える
            if ("Block" == other.tag)
            {
                Destroy(this.gameObject);
            }

        }
    }
}