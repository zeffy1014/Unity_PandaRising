using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace InputProvider
{
    // タッチ・マウス共通の移動情報(操作速度と位置)
    public class MoveInfo
    {
        public MoveInfo(Vector2 speed, Vector2 position)
        {
            MoveSpeed = speed;
            MovePosition = position;
        }

        public Vector2 MoveSpeed { get; private set; }
        public Vector2 MovePosition { get; private set; }
    }

    public interface IInputProvider
    {
        IObservable<bool> OnShot { get; }
        IObservable<float> OnThrow { get; }
        IObservable<Unit> OnBomb { get; }
        IObservable<Unit> OnMenu { get; }
        IObservable<float> OnSpeedEdit { get; }
        IObservable<MoveInfo> OnMovePlayer { get; }

    }
}