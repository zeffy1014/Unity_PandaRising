using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBase;
using Enemy;

namespace Bullet
{
    public class Bullet_Player_Block : Bullet
    {
        public override void OnTriggerEnter2D(Collider2D other)
        {
            // 壁に接触したら消える
            if ("Wall" == other.tag)
            {
                Destroy(this.gameObject);
            }
            // 敵に接触したらダメージを与える(消えない)
            if ("Enemy" == other.tag)
            {
                other.GetComponent<EnemyBase>().OnDamage(attack);
            }
            // 障害物に接触したら消える
            if ("Block" == other.tag)
            {
                Destroy(this.gameObject);
            }

        }
    }
}