﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


public class SignalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        /***** Signal関連 **********************************************************************/
        SignalBusInstaller.Install(Container);

        // Signal定義
        Container.DeclareSignal<GameStartSignal>();
        Container.DeclareSignal<FishLostSignal>();
        Container.DeclareSignal<DefeatEnemySignal>();

        // Signal受信時の処理
        Container.BindSignal<GameStartSignal>().ToMethod<GameController>(gc => gc.OnGameStart).FromResolve();
        Container.BindSignal<DefeatEnemySignal>().ToMethod<GameController>(gc => gc.OnDefeatEnemy).FromResolve();
        Container.BindSignal<FishLostSignal>().ToMethod<Player>(player => player.OnFishLost).FromResolve();

    }
}
