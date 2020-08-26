using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DataBase;
using Enemy;

public class GeneralInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        //Container.Bind<DataLibrarian>().AsCached(); Singletonにした

        // PresenterのBind 参照されないのでNonLazyで生成
        Container.Bind<EnemyGenerator>().AsSingle().NonLazy();
    }
}
