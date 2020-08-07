using UnityEngine;
using InputProvider;
using Zenject;
using UI;

public class InputInstaller : MonoInstaller<InputInstaller>
{
    [SerializeField] Player player = default;
    [SerializeField] bool unityRemote = false;    // �e�X�g�p UnityRemote�œ���m�F����Ƃ���Inspecter���true�ɂ���

    public override void InstallBindings()
    {
        // �g�ђ[����������^�b�`����
        if (true == PlatformInfo.IsMobile() || true == unityRemote)
        {
            Debug.Log("Bind TouchInputProvider as IInputProvider");
            Container.Bind<IInputProvider>().To<TouchInputProvider>().AsCached();
            Container.Bind<TouchOperation>().FromNewComponentOnNewGameObject().AsCached();
        }
        // �����łȂ���΃}�E�X+�L�[�{�[�h����
        else
        {
            Debug.Log("Bind PCInputProvider as IInputProvider");
            Container.Bind<IInputProvider>().To<PCInputProvider>().AsCached();
            Container.Bind<MouseOperation>().FromNewComponentOnNewGameObject().AsCached();
        }

        // ���͎󂯕t���N���X��Bind �Q�Ƃ���Ȃ��̂�NonLazy�Ő���
        Container.Bind<InputPresenter>().AsSingle().NonLazy();

        // Player��Bind Inspector�Ŏw�肵��Player���g��
        Container.Bind<Player>().FromInstance(player).AsCached();
    }
}
