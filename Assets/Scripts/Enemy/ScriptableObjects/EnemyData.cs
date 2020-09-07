using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy;

[CreateAssetMenu(menuName = "MyScriptable/Create EnemyData")]
public class EnemyData : ScriptableObject
{
    // 敵の種類によって決まるものたち
    public EnemyType type;      // 敵の種類
    public float life;          // ライフ
    public float moveSpeed;     // 移動速度
}
