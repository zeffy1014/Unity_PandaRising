using UnityEngine;
using InputProvider;
using Zenject;
using UGUI;

public class InputInstaller : MonoInstaller
{
    [SerializeField] bool unityRemote = false;    // テスト用 UnityRemoteで動作確認するときはInspecter上でtrueにする

    public override void InstallBindings()
    {
        // 携帯端末だったらタッチ操作
        if (true == PlatformInfo.IsMobile() || true == unityRemote)
        {
            Debug.Log("Bind TouchInputProvider as IInputProvider");
            Container.Bind<IInputProvider>().To<TouchInputProvider>().AsCached();
            Container.Bind<TouchOperation>().FromNewComponentOnNewGameObject().AsCached();
        }
        // そうでなければマウス+キーボード操作
        else
        {
            Debug.Log("Bind PCInputProvider as IInputProvider");
            Container.Bind<IInputProvider>().To<PCInputProvider>().AsCached();
            Container.Bind<MouseOperation>().FromNewComponentOnNewGameObject().AsCached();
            Container.Bind<KeyOperation>().FromNewComponentOnNewGameObject().AsCached();
        }

        // 入力受け付けクラスのBind 参照されないのでNonLazyで生成
        Container.Bind<InputPresenter>().AsSingle().NonLazy();

    }
}
