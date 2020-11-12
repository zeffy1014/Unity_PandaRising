using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBase;
using Enemy;

namespace Bullet
{
    public class Bullet_Player_Block : Bullet
    {
        float currentAttack = default;

        void Start()
        {
            currentAttack = attack;
        }

        public override void OnTriggerEnter2D(Collider2D other)
        {
            // 壁に接触したら消える
            if ("Wall" == other.tag)
            {
                Destroy(this.gameObject);
            }
            // 敵に接触した場合は特に何もしない(敵側で処理します)
            if ("Enemy" == other.tag)
            {
                // do nothing
            }
            // 障害物に接触したら消える
            if ("Block" == other.tag)
            {
                Destroy(this.gameObject);
            }

        }

        // 敵に接触して削れる
        public void OnShaved(float damage)
        {
            currentAttack -= damage;

            if (0.0f >= currentAttack)
            {
                // 削りきられたら消える
                Destroy(this.gameObject);
            }
            else
            {
                // 威力減り具合に応じてサイズが小さくなる
                float scale = currentAttack / attack;
                transform.localScale = new Vector3(scale, scale);
            }
        }
    }
}