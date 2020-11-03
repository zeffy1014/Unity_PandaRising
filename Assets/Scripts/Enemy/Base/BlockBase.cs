using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bullet;

namespace Enemy
{
    public class BlockBase : EnemyBase
    {
        // 動的に変わるもの
        protected float fixedAngle = default;                      // 落ちる方向
        [SerializeField] protected float speedRateInTouch = 0.0f;  // 接触中の落下速度比率(影響)
        protected float speedRateInFrame = 1.0f;                   // このフレームにおける速度倍率 接触中の速度低減に使う

        [SerializeField] float boundSpeed = 10.0f;    // 弾が当たった際に上に弾かれる速度(強さ)
        [SerializeField] float boundTime = 1.0f;      // 弾かれる時間
        float boundRemainTime = 0.0f;                 // 弾かれ時間残り

        // 種別で決まるもの
        [SerializeField] protected BulletType bulletType = default;

        /***** MonoBehaviourイベント処理 ****************************************************/
        public virtual new void Start()
        {
            // 進む(落ちる)方向を固定しておく
            fixedAngle = ANGLE_GAP + this.transform.eulerAngles.z;

        }
        public virtual new void Update()
        {
            // 移動する
            GoStraight(GetMoveSpeed() * speedRateInFrame);
            // 回転する
            transform.Rotate(new Vector3(0.0f, 0.0f, GetRotateSpeed() * Time.deltaTime * speedRateInFrame));

            // (弾かれる時間中ならば)上に弾かれる 残り時間に応じて弱くなる(また降ってくる)
            if (0.0f < boundRemainTime) {
                boundRemainTime = (Time.deltaTime < boundRemainTime)
                    ? boundRemainTime - Time.deltaTime
                    : 0.0f;
                Bound2Sky(boundSpeed * boundRemainTime);
            }

        }

        /***** Collider2Dイベント処理 ****************************************************/
        public virtual new void OnTriggerEnter2D(Collider2D other)
        {
            // 後逸検出したらHouseに弾を出して消える
            if ("WallMiss" == other.tag)
            {
                ShotBullet2House(300); //TODO:とりあえず固定ダメージ
                Destroy(this.gameObject);
            }

            // 弾が当たったら上に弾かれる タイマー開始
            if ("Bullet" == other.tag) boundRemainTime = boundTime;

        }
        public virtual void OnTriggerStay2D(Collider2D other)
        {
            // Playerが接触していたら速度に影響する
            if ("Player_Mantle" == other.tag)
            {
                // 基本的には動かなくする そうでない種別のBlockは派生先で実装
                speedRateInFrame = speedRateInTouch;

                // Player側でBlock弾入手するかも
                if (true == other.GetComponentInParent<Player>().OnTouchBlock(bulletType))
                {
                    Destroy(this.gameObject);
                }
            }
        }
        public virtual void OnTriggerExit2D(Collider2D other)
        {
            // Player接触から外れたら速度戻る
            if ("Player_Mantle" == other.tag) speedRateInFrame = 1.0f;
        }

        /***** 移動・回転処理 ****************************************************/
        // まっすぐ進む 回転しても真下に落ちる
        protected new void GoStraight(float speed)
        {
            transform.Translate(new Vector2(Mathf.Cos(fixedAngle * Mathf.Deg2Rad), Mathf.Sin(fixedAngle * Mathf.Deg2Rad)) * speed * Time.deltaTime, Space.World);
            return;
        }

        // 真上に弾かれる
        protected void Bound2Sky(float speed)
        {
            transform.Translate(new Vector2(0.0f, Mathf.Sin(ANGLE_GAP * Mathf.Deg2Rad)) * speed * Time.deltaTime, Space.World);
            return;
        }
    }
}