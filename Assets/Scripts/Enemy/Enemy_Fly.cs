using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class Enemy_Fly : EnemyBase
    {
        // 弾関連
        [SerializeField] float shotInterval = 1.0f;  // 発射間隔
        float shotElaspedTime = 0.0f;                // 前回発射からの経過時間

        /***** MonoBehaviourイベント処理 ****************************************************/
        // Update is called once per frame
        public override void Update()
        {
            base.Update();

            // ハエはまっすぐ進む
            GoStraight(enemyData.moveSpeed);

            // 一定時間経過のたびに発射
            shotElaspedTime += Time.deltaTime;
            if (shotInterval < shotElaspedTime)
            {
                ShotBullet2Player();
                shotElaspedTime = 0.0f;
            }
        }
    }
}
