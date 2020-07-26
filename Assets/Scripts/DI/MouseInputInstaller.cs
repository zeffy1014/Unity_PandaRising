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
                .Bind<IInputProvider>()               // IInputProvider���v�����ꂽ��
                .To<MouseInputProvider>()             // MouseInputProvider�𐶐����Ē�������
                .FromNewComponentOnNewGameObject()    // GameObject��V�K�������đΉ�
                .AsCached();                          // MouseInputProvider�������ς݂Ȃ�g����
        }
    }
}