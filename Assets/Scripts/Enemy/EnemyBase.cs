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

        // 敵の種類に応じて決まるもの TODO:ScriptableObject参照させる
        [SerializeField] protected EnemyType enemyType;
        [SerializeField] protected float moveSpeed;
        [SerializeField] protected float rotateSpeed;
        [SerializeField] protected int maxHp;

        // 動的に変わるもの
        float elaspedActionTime = 0.0f;        // 出現からの経過時間
        int nowHp;                             // 現在のHP

        // Generatorから渡される情報
        int id;                 // ユニークなID
        float activityTime;     // 活動時間(この時間経過で離脱開始)

        // ダメージ時のエフェクト
        public Material damageMaterial;    // ダメージ中にスプライトに適用するマテリアル(白くするため)
        public Material normalMaterial;    // 通常時のマテリアル(もとに戻すため)

        /***** static変数セット *************************************************************/
        public static void SetPlayer(Player playerIn) { player = playerIn; }
        public static void SetGameArea(GameArea area) { gameArea = area; }

        /***** MonoBehaviourイベント処理 ****************************************************/
        public void Start()
        {
            // HP初期化
            nowHp = maxHp;

            // TODO:生成から一定時間は無敵にする
            // this.GetComponent<CapsuleCollider2D>().enabled = false;

        }

        public virtual void Update()
        {
            // 派生先で実装
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
    }
}