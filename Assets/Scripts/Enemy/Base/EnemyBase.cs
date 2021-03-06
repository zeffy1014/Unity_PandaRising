﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Bullet;

// 撃破時のSignal
public class DefeatEnemySignal {
    public int dropMoney;
    public int baseScore;
}

namespace Enemy
{
    public class EnemyBase : MonoBehaviour
    {
        // 固定値
        protected const float ANGLE_GAP = 90.0f;
        const float FALLDOWN_SPEED = -20.0f;       // 撃破して落下する際の速度に使う

        // 共通で必要に応じて参照するもの
        static Player player;
        static House house;
        static GameArea gameArea;
        static float damageFlashTime = 0.1f;      // ダメージ表示する時間
        static BulletGenerator bulletGenerator;
        static float speedMagnification = 1.0f;   // 速度倍率(上昇速度による影響)

        public static void SetPlayer(Player playerIn) { player = playerIn; }
        public static void SetHouse(House houseIn) { house = houseIn; }
        public static void SetGameArea(GameArea area) { gameArea = area; }
        public static void SetBulletGenerator(BulletGenerator bgIn) { bulletGenerator = bgIn; }
        public static void UpdateSpeedMagnification(float mag) { speedMagnification = mag; }

        // 敵の種類に応じて決まるもの
        [SerializeField] protected EnemyData enemyData;
        [SerializeField] protected GameObject defeatedEffect;   // 撃破時のエフェクト
        [SerializeField] protected SEList defeatedSound;        // 撃破時の音

        // 速度取得
        public float GetMoveSpeed() { return enemyData.moveSpeed * speedMagnification; }  // 上昇速度の影響を受ける
        public float GetRotateSpeed() { return enemyData.rotateSpeed; }

        // 動的に変わるもの
        float elaspedActionTime = 0.0f;        // 出現からの経過時間
        float nowHp;                           // 現在のHP
        bool defeated = false;                 // 撃破されたかどうか(撃破時の処理が複数回呼ばれないように管理)

        // Generatorから渡される情報
        protected int id;                 // ユニークなID
        float activityTime;     // 活動時間(この時間経過で離脱開始)
        float undefeatableTime; // 出現から一定時間の無敵時間
        public void SetGenerateInfo(int idIn, float activityTimeIn, float undefeatableTimeIn)
        {
            id = idIn;
            activityTime = activityTimeIn;
            undefeatableTime = undefeatableTimeIn;
        }

        // ダメージ時のエフェクト
        public Material damageMaterial;    // ダメージ中にスプライトに適用するマテリアル(白くするため)
        public Material normalMaterial;    // 通常時のマテリアル(もとに戻すため)

        // このClassはSignalを発行する
        static SignalBus signalBus;
        public static void SetSignalBus(SignalBus sb) { signalBus = sb; }

        /***** MonoBehaviourイベント処理 ****************************************************/
        public virtual void Start()
        {
            // HP初期化
            nowHp = enemyData.maxHp;

            // 生成から一定時間は無敵にする
            this.GetComponent<CapsuleCollider2D>().enabled = false;

        }

        public virtual void Update()
        {
            elaspedActionTime += (Time.deltaTime * speedMagnification); // 上昇速度倍率の影響を受ける

            // 無敵時間終了
            if (undefeatableTime <= elaspedActionTime)
            {
                this.GetComponent<CapsuleCollider2D>().enabled = true;
            }

            // あとは派生先で実装
        }

        /***** Collider2Dイベント処理 ****************************************************/
        public virtual void OnTriggerEnter2D(Collider2D other)
        {
            // Playerに接触したら消える
            if ("Player" == other.tag)
            {
                // Playerが無敵時間でなければ
                if (!other.GetComponent<Player>().BeingDamaged)
                {
                    // 撃破されてスコアとお金を加算して消える
                    Vector2 fallSpeed = new Vector2(0.0f, FALLDOWN_SPEED * speedMagnification);
                    var effect = Instantiate(defeatedEffect, transform.position, Quaternion.identity);
                    effect.GetComponent<Rigidbody2D>().AddForce(fallSpeed, ForceMode2D.Impulse);

                    // 撃破時の処理
                    DefeatedAction();

                    Destroy(this.gameObject);

                }
            }

            // 後逸検出したらHouseに弾を出して消える
            if ("WallMiss" == other.tag)
            {
                ShotBullet2House(enemyData.houseAttack); //ダメージは敵によって変わる
                Destroy(this.gameObject);
            }

            // Block接触したら削る
            if ("Bullet_Player_Block" == other.tag)
            {
                var bullet = other.GetComponent<Bullet_Player_Block>();
                float damage = bullet.GetAttack();

                // Blockを削ってから己にダメージ
                bullet.OnShaved(this.nowHp);
                OnDamage(damage);
            }

        }

        /***** 移動・回転処理 ****************************************************/
        // まっすぐ進む
        protected void GoStraight(float speed)
        {
            transform.Translate(new Vector2(Mathf.Cos(GetNowAngle() * Mathf.Deg2Rad), Mathf.Sin(GetNowAngle() * Mathf.Deg2Rad)) * speed * Time.deltaTime, Space.World);
            return;
        }

        // 現在の角度取得(頭が向いている方向)
        protected float GetNowAngle()
        {
            return ANGLE_GAP + transform.eulerAngles.z;
        }

        // 現在の角度指定(頭が向いている方向)
        protected void SetNowAngle(float angle)
        {
            Vector3 setAngle = transform.eulerAngles;
            setAngle.z = angle - ANGLE_GAP;
            transform.eulerAngles = setAngle;

            return;
        }

        /***** 弾を出す処理 ****************************************************/
        // 指定された角度で弾を撃つ
        protected void ShotBullet(
            BulletType type,
            float deg,
            float? speed = null, float? accel = null, float? size = null, Color? color = null)
        {
            // 発射位置と向き
            Vector3 genPos = this.transform.position;
            Vector3 genRot = this.transform.eulerAngles;

            bulletGenerator.ShotBullet(
                genPos, genRot, type, deg,
                moveSpeed:speed,
                accel:accel,
                size:size,
                color:color);
        }

        // 自機に向かって弾を撃つ
        protected void ShotBullet2Player(
            BulletType type = BulletType.Enemy_Circle_Straight, 
            float? speed = null, float? accel = null, float? size = null, Color? color = null)
        {
            // 自機に対する角度を算出
            Vector2 posDiff = player.transform.position - this.transform.position;
            float targetAngle = Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg;

            ShotBullet(type, targetAngle, speed, accel, size, color);

            return;
        }

        // 家に向かって弾を撃つ(Enemy後逸時処理)
        protected void ShotBullet2House(float attack)
        {
            // 画面上部の固定位置から発射
            Vector3 genPos = gameArea.GetPosFromRate(new Vector2(1.2f, 1.1f));
            Vector3 genRot = Vector3.zero;

            // Houseに対する角度を算出
            Vector2 posDiff = house.transform.position - genPos;
            float targetAngle = Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg;

            // 攻撃力指定して発射
            bulletGenerator.ShotBullet(genPos, genRot, BulletType.FallDown, targetAngle, attack:attack);

            return;
        }

        // 複数弾(奇数弾・偶数弾)を撃つ
        // 弾の数と1つ1つの角度(デフォルト:30度だが弾数が多い場合は重ならないように調整が入る)を指定
        protected void ShotMultipleBullet(
            int bulletNum,
            float angle = 30.0f,
            BulletType type = BulletType.Enemy_Circle_Straight, 
            float? speed = null, float? accel = null, float? size = null, Color? color = null)
        {
            // 自機がいないなら撃たない
            if (null == player) return;

            // 自機に対する角度を算出
            Vector2 posDiff = player.transform.position - this.transform.position;
            float targetAngle = Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg;

            // 必要ならば角度調整
            if (360.0f < bulletNum * angle) angle = 360.0f / bulletNum;

            // 最初の弾の角度
            float startAngle = targetAngle - angle * (bulletNum / 2);
            if (0 == bulletNum % 2)
            {
                // 偶数弾だったら半分ずらす
                startAngle -= (angle / 2);
            }

            for (int i = 0; i < bulletNum; i++)
            {
                ShotBullet(type, startAngle + (angle * i), speed);
            }

            return;
        }

        /***** その他処理 ****************************************************/
        // ダメージ
        public void OnDamage(float damage)
        {
            nowHp -= damage;

            if (0.0f >= nowHp)
            {
                // 撃破
                Vector2 fallSpeed = new Vector2(0.0f, FALLDOWN_SPEED * speedMagnification);
                var effect = Instantiate(defeatedEffect, transform.position, Quaternion.identity);
                effect.GetComponent<Rigidbody2D>().AddForce(fallSpeed, ForceMode2D.Impulse);
                AudioController.Instance.PlaySE(defeatedSound);

                // 撃破時の処理
                DefeatedAction();

                Destroy(this.gameObject);
            }
            else
            {
                // TODO:ダメージ演出
                StartCoroutine(Flash(0.1f));
                AudioController.Instance.PlaySE(SEList.Enemy_Damage);
            }
        }

        // ちょっと光る
        IEnumerator Flash(float time)
        {
            this.gameObject.GetComponent<SpriteRenderer>().material = damageMaterial;
            yield return new WaitForSeconds(time);
            this.gameObject.GetComponent<SpriteRenderer>().material = normalMaterial;
        }

        // 撃破時の処理
        protected void DefeatedAction()
        {
            // 撃破処理されていなければ1回だけ実行する
            if (!defeated)
            {
                // Signal発行して所持金とスコア増加させる
                signalBus.Fire(new DefeatEnemySignal() { dropMoney = this.enemyData.dropMoney, baseScore = this.enemyData.baseScore });
                //Debug.Log("enemy defeated id:" + this.id);
                defeated = true;
            }
            else
            {
                //Debug.Log("enemy defeated but already processed id:" + this.id);
            }
        }
    }
}