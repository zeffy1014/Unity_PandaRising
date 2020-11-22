using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class Enemy_Fly : EnemyBase
    {
        // 弾関連
        [SerializeField] float shotInterval = 5.0f;  // 発射間隔
        float shotElaspedTime = 0.0f;                // 前回発射からの経過時間

        /***** MonoBehaviourイベント処理 ****************************************************/
        public override void Update()
        {
            base.Update();

            // ハエはまっすぐ進む
            GoStraight(GetMoveSpeed());

            // 一定時間経過のたびに発射
            shotElaspedTime += Time.deltaTime;
            if (shotInterval < shotElaspedTime)
            {
                //ShotBullet2Player(accel:5.0f, size:1.5f, color:Color.blue);  // 加速度あり・サイズと色をデフォルトから変えて放つ
                AudioController.Instance.PlaySE(SEList.Enemy_Shot_01, 1);    // 発射音 大量に発生するかもしれないのでチャンネル指定
                shotElaspedTime = 0.0f;
            }
        }
    }
}
