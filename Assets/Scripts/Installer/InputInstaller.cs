using UnityEngine;
using Player;
using InputProvider;
using Zenject;

public class InputInstaller : MonoInstaller<InputInstaller>
{
    [SerializeField] Player.Player player = default;
    [SerializeField] bool unityRemote = false;    // �e�X�g�p UnityRemote�œ���m�F����Ƃ���Inspecter���true�ɂ���

    public override void InstallBindings()
    {
        Container
            .Bind<IInputProvider>()               // IInputProvider���v�����ꂽ��
            .To<PCInputProvider>()                // PCInputProvider�𐶐����Ē�������
            .AsCached();                          // PCInputProvider�������ς݂Ȃ�g����

        // ���͎󂯕t���N���X��Bind �Q�Ƃ���Ȃ��̂�NonLazy�Ő���
        Container.Bind<InputPresenter>().AsSingle().NonLazy();

        // Player��Bind Inspector�Ŏw�肵��Player���g��
        Container
            .Bind<Player.Player>()
            .FromInstance(player)
            .AsCached();
    }
}
