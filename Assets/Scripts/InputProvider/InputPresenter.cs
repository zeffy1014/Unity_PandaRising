using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputProvider;
using UniRx;

public class InputPresenter
{
    private Player.Player player;
    private IInputProvider pcInput;

    InputPresenter(IInputProvider pcInput, Player.Player player)
    {
        this.player = player;
        this.pcInput = pcInput;

        // 各種操作を監視してPlayerを動かす
        this.pcInput.OnShot.Subscribe(_ => player.Shot());
        this.pcInput.OnThrow.Subscribe(angle => player.Throw(angle));
        this.pcInput.OnBomb.Subscribe(_ => player.Bomb());
        this.pcInput.OnMovePlayer.Subscribe(speed => player.MovePlayer(speed));
    }

}
