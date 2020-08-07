using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputProvider;
using UniRx;

// 各種操作を受けて最終的にPlayerやGameControllerに処理の指示を出すひと
public class InputPresenter
{
    private Player player;
    private IInputProvider input;

    InputPresenter(IInputProvider input, Player player)
    {
        this.player = player;
        this.input = input;

        // 各種操作を監視してPlayerを動かす
        this.input.OnShot.Subscribe(_ => player.Shot());
        this.input.OnThrow.Subscribe(angle => player.Throw(angle));
        this.input.OnBomb.Subscribe(_ => player.Bomb());

        this.input.OnMovePlayer
            .Where(info => Vector2.zero != info.MoveSpeed)
            .Subscribe(info => player.MovePlayer(info));

        // 各種操作を監視してGameControllerへ通知する
        this.input.OnMenu.Subscribe(_ => { });  // TODO:後で実装

    }

}
