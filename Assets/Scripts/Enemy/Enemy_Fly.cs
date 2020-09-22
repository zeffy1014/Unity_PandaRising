using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class Enemy_Fly : EnemyBase
    {
        /***** MonoBehaviourイベント処理 ****************************************************/
        // Update is called once per frame
        public override void Update()
        {
            base.Update();

            // ハエはまっすぐ進む
            GoStraight(moveSpeed);

        }
    }
}
