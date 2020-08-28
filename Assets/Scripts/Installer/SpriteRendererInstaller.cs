using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SpriteRendererInstaller : MonoInstaller
{
    [SerializeField] GameObject target;  // Bindさせる対象GameObject

    public override void InstallBindings()
    {
        // SpriteRenderer を生成してtargetに追加してBind
        Container.Bind<SpriteRenderer>().FromNewComponentOn(target).AsCached();

    }
}
