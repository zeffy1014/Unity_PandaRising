using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DataBase;

public class GeneralInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        //Container.Bind<DataLibrarian>().AsCached();
    }
}
