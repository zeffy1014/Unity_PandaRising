using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBase;

namespace Bullet
{
    public class Bullet_Player_Fish : Bullet
    {
        // static変数として威力を取得・保持しておく
        static float power = default;
        new static float size = default;
        static float toughness = default;  // 場に出ていられる時間
        static Bullet_Player_Fish()
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
                Debug.Log("Bullet_Player_Fish LoadData failed... use default power...");
                power = default;
            }
            else
            {
                // Fishのサイズや威力を設定
                power = rtInfo.GetFishPower(userData.GetLevel(ReinforceTarget.FishPower));
                size = rtInfo.GetFishSize(userData.GetLevel(ReinforceTarget.FishPower));
                toughness = rtInfo.GetFishToughness(userData.GetLevel(ReinforceTarget.FishToughness));

            }
        }


        // パラメータ指定して発射 派生動作
        public override void Shot(float angle)
        {
            // 基底動作で速度設定・RigidBody2D取得など
            base.Shot(angle);

            // 発射
            body.AddForce(moveSpeed2D, ForceMode2D.Impulse);
            body.AddTorque(this.rotateSpeed, ForceMode2D.Impulse);

        }

        public override void OnTriggerEnter2D(Collider2D other)
        {
            // 壁にあたっても消えない

        }
    }
}
