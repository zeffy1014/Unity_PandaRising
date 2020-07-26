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
                .Bind<IInputProvider>()               // IInputProvider‚ª—v‹‚³‚ê‚½‚ç
                .To<MouseInputProvider>()             // MouseInputProvider‚ğ¶¬‚µ‚Ä’“ü‚·‚é
                .FromNewComponentOnNewGameObject()    // GameObject‚ğV‹K¶¬‚µ‚Ä‘Î‰
                .AsCached();                          // MouseInputProvider‚ª¶¬Ï‚İ‚È‚çg‚¢‰ñ‚·
        }
    }
}