using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ILoadData
{
    // Load完了監視用プロパティ
    IReadOnlyReactiveProperty<bool> OnLoadCompleteProperty { get; }

    // Load完了確認IF
    bool LoadCompleted();

}
