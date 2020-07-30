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
                .Bind<IInputProvider>()               // IInputProvider���v�����ꂽ��
                .To<MouseInputProvider>()             // MouseInputProvider�𐶐����Ē�������
                .FromNewComponentOnNewGameObject()    // GameObject��V�K�������đΉ�
                .AsCached();                          // MouseInputProvider�������ς݂Ȃ�g����

            // ���͎󂯕t���N���X��Bind �Q�Ƃ���Ȃ��̂�NonLazy�Ő���
            Container.Bind<InputPresenter>().AsSingle().NonLazy();
            Container.Bind<PCInputProvider>().AsSingle().NonLazy();

            // Player��Bind Inspector�Ŏw�肵��Player���g��
            Container
                .Bind<Player.Player>()
                .FromInstance(player)
                .AsCached();
        }
    }
}