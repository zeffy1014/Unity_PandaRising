using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    [SerializeField] GameObject[] lifeMarks;

    // ライフ数更新
    public void UpdateLife(int life)
    {
        // 範囲チェック
        if (0 > life || lifeMarks.Length < life)
        {
            // とりあえずログだけ
            Debug.Log("Update life out of range... max:" + lifeMarks.Length + ", min:0");
        }

        // 指定数まで表示ON
        for (int i = 0; i < lifeMarks.Length; i++)
        {
            if (life > i) lifeMarks[i].SetActive(true);
            else lifeMarks[i].SetActive(false);
        }
    }
}
