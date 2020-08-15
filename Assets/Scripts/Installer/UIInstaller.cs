using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UGUI;
using Zenject;

public class UIInstaller : MonoInstaller<InputInstaller>
{
    [SerializeField] ThrowButtonView tButtonView;
    [SerializeField] BombButtonView bButtonView;
    [SerializeField] PlayerLife pLife;
    [SerializeField] HouseLife hLife;
    [SerializeField] BossLife bLife;
    [SerializeField] SpeedInfo sInfo;
    [SerializeField] HightInfo hInfo;
    [SerializeField] Combo combo;
    [SerializeField] PlayTime time;
    [SerializeField] Score score;
    [SerializeField] Money money;

    [SerializeField] House house;
    [SerializeField] GameController gameController;

    public override void InstallBindings()
    {
        // PresenterのBind 参照されないのでNonLazyで生成
        Container.Bind<UIPresenter>().AsSingle().NonLazy();

        // 各種uGUI系のBind Inspectorで指定したPlayerを使う
        Container.Bind<ThrowButtonView>().FromInstance(tButtonView).AsCached();
        Container.Bind<BombButtonView>().FromInstance(bButtonView).AsCached();
        Container.Bind<PlayerLife>().FromInstance(pLife).AsCached();
        Container.Bind<HouseLife>().FromInstance(hLife).AsCached();
        Container.Bind<BossLife>().FromInstance(bLife).AsCached();
        Container.Bind<SpeedInfo>().FromInstance(sInfo).AsCached();
        Container.Bind<HightInfo>().FromInstance(hInfo).AsCached();
        Container.Bind<Combo>().FromInstance(combo).AsCached();
        Container.Bind<PlayTime>().FromInstance(time).AsCached();
        Container.Bind<Score>().FromInstance(score).AsCached();
        Container.Bind<Money>().FromInstance(money).AsCached();

        // House, GameControllerもBind
        Container.Bind<House>().FromInstance(house).AsCached();
        Container.Bind<GameController>().FromInstance(gameController).AsCached();
        
        // Playerは別のInstallerでBindするのでここには書かない

    }
}