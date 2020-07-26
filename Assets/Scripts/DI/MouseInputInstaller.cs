using UnityEngine;
using Player;
using InputDAL;
using Zenject;

namespace DI
{
    public class MouseInputInstaller : MonoInstaller<MouseInputInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<IInputProvider>()               // IInputProviderが要求されたら
                .To<MouseInputProvider>()             // MouseInputProviderを生成して注入する
                .FromNewComponentOnNewGameObject()    // GameObjectを新規生成して対応
                .AsCached();                          // MouseInputProviderが生成済みなら使い回す
        }
    }
}