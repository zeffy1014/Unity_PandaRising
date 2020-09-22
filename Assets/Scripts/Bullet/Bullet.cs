using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bullet
{
    public class Bullet : MonoBehaviour
    {
        // 弾の種別を持つ
        public BulletType Type = default;

        // 各種パラメータ デフォルト値を持っている
        [SerializeField] protected float moveSpeed = default;   // 移動速度
        [SerializeField] protected float rotateSpeed = default; // 回転速度
        [SerializeField] protected float accel = default;       // 加速度
        [SerializeField] protected float size = default;        // サイズ
        [SerializeField] protected Color color = default;       // 色

        // 必ず外部指定するもの
        float angle = default;  // 角度

        // その他
        protected Vector2 moveSpeed2D;    // 角度を加味した最終的な移動速度
        protected Rigidbody2D body;

        // パラメータ上書きがあれば実施
        // default引数はNullable<T>を使ってnull扱いとする
        public virtual void SetParam(float? moveSpeed = null, float? rotateSpeed = null, float? accel = null, float? size = null, Color? color = null)
        {
            // パラメータ変更がある場合は反映
            if (null != moveSpeed) this.moveSpeed = (float)moveSpeed;
            if (null != rotateSpeed) this.rotateSpeed = (float)rotateSpeed;
            if (null != accel) this.accel = (float)accel;
            if (null != size) this.size = (float)size;
            if (null != color) this.color = (Color)color;

            return;
        }

        // パラメータ指定して発射 基底動作
        // default引数はNullable<T>を使ってnull扱いとする
        public virtual void Shot(float angle)
        {
            // 発射角度から初期速度を出す
            this.angle = angle;
            float speedX = this.moveSpeed * Mathf.Cos(this.angle * Mathf.Deg2Rad);
            float speedY = this.moveSpeed * Mathf.Sin(this.angle * Mathf.Deg2Rad);
            moveSpeed2D = new Vector2(speedX, speedY);

            body = this.GetComponent<Rigidbody2D>();

            // 以降は派生先で処理(AddForceしたり色やサイズを変えたり)
        }

        public virtual void FixedUpdate()
        {
            // 派生先で処理
        }

        public virtual void OnTriggerEnter2D(Collider2D other)
        {
            // 基底動作
            if ("Bullet_Enemy" == this.tag)
            {
                if ("Player" == other.tag || "Wall" == other.tag)
                {
                    // 敵弾はPlayerか壁にあたったら消える そうでないものがあったら派生先で実装
                    Destroy(this.gameObject);
                }
            }
            if ("Bullet" == this.tag)
            {
                // Playerの弾は派生先で処理を実装
            }
        }

    }
}