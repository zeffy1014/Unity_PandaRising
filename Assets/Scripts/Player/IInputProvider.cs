using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public interface IInputProvider
    {
        bool GetShot();                   // ショット操作がされているか
        bool GetThrow(ref float angle);   // 投げ操作がされているか(angleは角度(0.0f-360.f deg), -1.0fは方角指定なしとする)
        Vector2 GetMoveSpeed();           // 移動操作の速さ
    }
}