using UnityEngine;
using Player;
using InputProvider;
using Zenject;

namespace DI
{
    public class MouseInputInstaller : MonoInstaller<MouseInputInstaller>
    {
        [SerializeField] Player.Player player;

        public override void InstallBindings()
        {
            Container
                .Bind<IInputProvider>()               // IInputProviderが要求されたら
                .To<MouseInputProvider>()             // MouseInputProviderを生成して注入する
                .FromNewComponentOnNewGameObject()    // GameObjectを新規生成して対応
                .AsCached();                          // MouseInputProviderが生成済みなら使い回す

            // 入力受け付けクラスのBind 参照されないのでNonLazyで生成
            Container.Bind<InputPresenter>().AsSingle().NonLazy();
            Container.Bind<PCInputProvider>().AsSingle().NonLazy();

            // PlayerのBind Inspectorで指定したPlayerを使う
            Container
                .Bind<Player.Player>()
                .FromInstance(player)
                .AsCached();
        }
    }
}