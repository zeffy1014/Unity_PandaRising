using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class EnemyBase : MonoBehaviour
    {
        // 固定値
        const float ANGLE_GAP = 90.0f;

        // 共通で必要に応じて参照するもの
        static Player player;
        static GameArea gameArea;
        static float damageFlashTime = 0.1f;  // ダメージ表示する時間
        public static void SetPlayer(Player playerIn) { player = playerIn; }
        public static void SetGameArea(GameArea area) { gameArea = area; }

        // 敵の種類に応じて決まるもの
        [SerializeField] protected EnemyData enemyData;

        // 動的に変わるもの
        float elaspedActionTime = 0.0f;        // 出現からの経過時間
        float nowHp;                           // 現在のHP

        // Generatorから渡される情報
        int id;                 // ユニークなID
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


        /***** MonoBehaviourイベント処理 ****************************************************/
        public void Start()
        {
            // HP初期化
            nowHp = enemyData.maxHp;

            // 生成から一定時間は無敵にする
            this.GetComponent<CapsuleCollider2D>().enabled = false;

        }

        public virtual void Update()
        {
            elaspedActionTime += Time.deltaTime;

            // 無敵時間終了
            if (undefeatableTime <= elaspedActionTime)
            {
                this.GetComponent<CapsuleCollider2D>().enabled = true;
            }

            // あとは派生先で実装
        }

        /***** Collider2Dイベント処理 ****************************************************/
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Playerに接触したら消える
            if ("Player" == other.tag)
            {
                Destroy(this.gameObject);
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


        /***** その他処理 ****************************************************/
        // ダメージ
        public void OnDamage(float damage)
        {
            nowHp -= damage;

            if (0.0f >= nowHp)
            {
                // 撃破
                // TODO:撃破演出
                //audioSource.PlayOneShot(defeatSE);
                //Instantiate<GameObject>(defeatEffect, transform.position, Quaternion.identity);
                Destroy(this.gameObject);
            }
            else
            {
                // TODO:ダメージ表現
                //audioSource.PlayOneShot(damageSE);
                //StartCoroutine(Flash(flashTime));
            }
        }
    }
}