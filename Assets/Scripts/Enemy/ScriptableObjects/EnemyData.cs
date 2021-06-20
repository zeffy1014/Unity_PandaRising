using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy;

[CreateAssetMenu(menuName = "MyScriptable/Create EnemyData")]
public class EnemyData : ScriptableObject
{
    // 敵の種類によって決まるものたち
    public EnemyType enemyType;     // 敵の種類
    public float maxHp;             // 最大ライフ
    public float moveSpeed;         // 移動速度
    public float rotateSpeed;       // 旋回速度
    public float houseAttack;       // 後逸時の家へのダメージ(落とす弾の攻撃力)
    public int dropMoney;           // 撃破時に得られるお金
    public int baseScore;           // 撃破時に得られるスコア(実際はコンボなどの倍率がかかる)
}
