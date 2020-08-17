﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputProvider;
using UniRx;

// 各種操作を受けて最終的にPlayerやGameControllerに処理の指示を出すひと
public class InputPresenter
{
    private Player player;
    private IInputProvider input;
    private GameController gameController;

    InputPresenter(IInputProvider input, Player player, GameController gameController)
    {
        this.player = player;
        this.input = input;
        this.gameController = gameController;

        // 各種操作を監視してPlayerを動かす
        this.input.OnShot.Subscribe(_ => this.player.Shot());
        this.input.OnThrow.Subscribe(angle => this.player.Throw(angle));
        this.input.OnBomb.Subscribe(_ => this.player.Bomb());

        this.input.OnMovePlayer
            .Where(info => Vector2.zero != info.MoveSpeed)
            .Subscribe(info => this.player.MovePlayer(info));

        // 各種操作を監視してGameControllerへ通知する
        this.input.OnMenu.Subscribe(_ => { });  // TODO:後で実装
        this.input.OnSpeedEdit.Subscribe(rate => this.gameController.UpdateSpeedMagnification(rate, EditMode.Rate_Delta));

    }

}
