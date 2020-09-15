using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


public class SignalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<Bullet.Bullet_Player_Fish>().FromNewComponentOnNewGameObject().AsCached();

        /***** Signal関連 **********************************************************************/
        SignalBusInstaller.Install(Container);

        // Signal定義
        Container.DeclareSignal<GameStartSignal>();
        Container.DeclareSignal<FishLostSignal>();

        // Signal受信時の処理
        Container.BindSignal<GameStartSignal>().ToMethod<GameController>(gc => gc.OnGameStart).FromResolve();
        Container.BindSignal<FishLostSignal>().ToMethod<Player>(gc => gc.OnFishLost).FromResolve();

    }
}
