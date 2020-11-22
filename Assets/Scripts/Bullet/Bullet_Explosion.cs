using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBase;
using Enemy;

namespace Bullet
{
    // 爆発の固定弾
    public class Bullet_Explosion : Bullet_Player_Block
    {
        // 爆発は一定時間で消える弾なので生存期間を設定する
        [SerializeField] float lifeTime = default;
        float elapsedTime = default;

        [SerializeField] GameObject explodeEffect = default;   // 爆発演出

        // パラメータ指定して発射 派生動作
        public override void Shot(float angle)
        {
            // 固定弾なので設置したら動かさない
            // 爆発演出してタイマー開始する
            Instantiate(explodeEffect, transform.position, Quaternion.identity);
            elapsedTime = 0.0f;

        }

        public override void FixedUpdate()
        {
            elapsedTime += Time.deltaTime;
            if (lifeTime <= elapsedTime)
            {
                // 一定時間経過したら消滅する
                Destroy(this.gameObject);
            }
        }

        public override void OnTriggerEnter2D(Collider2D other)
        {
            if ("Enemy" == other.tag)
            {
                other.GetComponent<EnemyBase>().OnDamage(attack);
            }
        }

        // 敵に接触しても削れないようにする
        public override void OnShaved(float damage)
        {
            // do nothing
        }
    }
}