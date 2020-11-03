using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bullet;

namespace Enemy
{
    public class Block_Rock : BlockBase
    {
        void Initialize()
        {
            bulletType = BulletType.Player_Block_Rock;
        }

    }
}