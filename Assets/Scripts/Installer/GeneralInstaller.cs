using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DataBase;
using Enemy;
using Bullet;

public class GeneralInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        //Container.Bind<DataLibrarian>().AsCached(); Singletonにした

        // 参照されないのでNonLazyで生成する
        Container.Bind<EnemyGenerator>().AsSingle().NonLazy();
        Container.Bind<BulletGenerator>().AsSingle().NonLazy();
    }
}
