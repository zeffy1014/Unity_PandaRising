using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace InputProvider
{
    public interface IInputProvider
    {
        IObservable<Unit> OnShot { get; }
        IObservable<float> OnThrow { get; }
        IObservable<Unit> OnBomb { get; }
        IObservable<Vector2> OnMovePlayer { get; }

    }
}