using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputProvider;
using UniRx;
using Zenject;

// ゲーム開始操作のSignal
public class GameStartSignal { }


// 各種操作を受けて最終的にPlayerやGameControllerに処理の指示を出すひと
public class InputPresenter
{
    private Player player;
    private IInputProvider input;
    private GameController gameController;

    // Signal発行します
    private SignalBus signalBus;
    private bool gameStartSignalFired = false;   // シーン読み込みのたび1回だけ発行する想定

    InputPresenter(IInputProvider input, Player player, GameController gameController, SignalBus signalBus)
    {
        this.player = player;
        this.input = input;
        this.gameController = gameController;
        this.signalBus = signalBus;

        // 各種操作を監視してPlayerを動かす
        this.input.OnShot.Subscribe(push => {
            this.player.Shot(push);

            // ゲーム開始操作を兼ねるためSignal発行(そのシーンで1回だけ)
            if (!gameStartSignalFired)
            {
                signalBus.Fire<GameStartSignal>();
                gameStartSignalFired = true;
            }
        });
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
