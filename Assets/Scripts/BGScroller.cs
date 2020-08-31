using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DataBase;
using UniRx;

public class BGScroller : MonoBehaviour, ILoadData
{
    GameController gameController;     // 現在高度監視用GameController
    SpriteRenderer spriteRenderer;     // 画像表示用SpriteRenderer

    int heightStart = default;
    int heightGoal = default;

    // 背景画像の初期位置と移動距離
    float initialPositionY = default;
    float moveDistanceY = default;

    // Zenject Method Injection
    [Inject]
    void Construct(GameController gc, SpriteRenderer sr)
    {
        gameController = gc;
        spriteRenderer = sr;
    }

    /***** 読み込み完了監視 **********************************************************************/
    ReactiveProperty<bool> _onLoadCompleteProperty = new ReactiveProperty<bool>(false);
    public IReadOnlyReactiveProperty<bool> OnLoadCompleteProperty => _onLoadCompleteProperty;
    public bool LoadCompleted() { return _onLoadCompleteProperty.Value; }


    /***** MonoBehaviourイベント処理 **********************************************/
    void Start()
    {
        // 各種設定・情報を読み込む
        bool loadResult = LoadData();
        if (true == loadResult)
        {
            // 読み込み完了したらフラグを立てる
            _onLoadCompleteProperty.Value = true;
        }
        else
        {
            // TODO:読み込み失敗したらエラー通知してメインメニューに戻る？
            Debug.Log("EnemyGenerator load data failed...");
        }

    }

    /***** BGScroller独自処理 *****************************************************/
    // 読み込み
    bool LoadData()
    {
        // ステージ構成情報から高度・画像読み込み
        var stageInfo = DataLibrarian.Instance.GetStageInfo(gameController.PlayingStage);

        Sprite image = Resources.Load<Sprite>(stageInfo.GetPathBackGroundImage());  // 画像
        heightStart = stageInfo.GetHeightStart();  // スタート地点高度
        heightGoal = stageInfo.GetHeightGoal();    // ゴール地点高度

        // 画像が読み込みできたら…
        if (null != image)
        {
            // 背景画像の初期位置・移動距離を設定
            SetInitialInfo(image);

            // 初期位置に背景画像表示
            Vector3 initPos = new Vector3(this.transform.position.x, initialPositionY, 0.0f);
            this.transform.position = initPos;
            spriteRenderer.sprite = image;
            spriteRenderer.sortingOrder = -2;
        }
        else
        {
            Debug.Log("BG Image cannot load...");
            return false;
        }

        // 現在高度を監視
        gameController.HeightReactiveProperty.DistinctUntilChanged()
            .Subscribe(height =>
            {
                ScrollImage(height);
            });

        return true;

    }

    // 初期設定
    void SetInitialInfo(Sprite image)
    {
        // 画像サイズと表示倍率から画像の高さを算出
        float ppu = image.pixelsPerUnit;
        float scale = this.transform.localScale.y;
        float imageHeight = (image.rect.height / ppu) * scale;  // これがゲーム画面上の画像の高さ

        // 画面の上下の高さを算出
        float screenHeight =
            Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0.0f)).y -
            Camera.main.ScreenToWorldPoint(Vector3.zero).y;

        // 背景スクロール必要な距離は画像高さ - 画面高さ
        moveDistanceY = imageHeight - screenHeight;

        // 背景初期位置はスクロール距離の半分だけ画像をY方向に進めた場所
        initialPositionY = moveDistanceY / 2.0f;

        Debug.Log("Initial BG Image position:" + initialPositionY + ", scroll distance:" + moveDistanceY);

        return;
    }

    // 現在高度に合わせて背景画像を移動
    void ScrollImage(int height)
    {
        // 現在高度が全体の何割の進み具合か計算
        float heightRate = (height - heightStart) / (heightGoal - heightStart);

        // スクロール距離に現在の進み具合をかけて初期位置から引く それが現在の背景の位置
        float nowPositionY = initialPositionY - (moveDistanceY * heightRate);
        Vector3 nowPos = new Vector3(this.transform.position.x, nowPositionY, 0.0f);

        // 背景移動
        this.transform.position = nowPos;

        return;

    }
}
