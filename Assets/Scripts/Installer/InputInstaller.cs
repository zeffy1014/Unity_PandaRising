using UnityEngine;
using Player;
using InputProvider;
using Zenject;

public class InputInstaller : MonoInstaller<InputInstaller>
{
    [SerializeField] Player.Player player = default;
    [SerializeField] bool unityRemote = false;    // テスト用 UnityRemoteで動作確認するときはInspecter上でtrueにする

    public override void InstallBindings()
    {
        Container
            .Bind<IInputProvider>()               // IInputProviderが要求されたら
            .To<PCInputProvider>()                // PCInputProviderを生成して注入する
            .AsCached();                          // PCInputProviderが生成済みなら使い回す

        // 入力受け付けクラスのBind 参照されないのでNonLazyで生成
        Container.Bind<InputPresenter>().AsSingle().NonLazy();

        // PlayerのBind Inspectorで指定したPlayerを使う
        Container
            .Bind<Player.Player>()
            .FromInstance(player)
            .AsCached();
    }
}
