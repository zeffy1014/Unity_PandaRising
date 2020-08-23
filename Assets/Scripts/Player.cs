using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Zenject;
using InputProvider;

public class Player : MonoBehaviour
{
    // 固定値
    const float ANGLE_GAP = 90.0f;      // デフォルトで上(90.0deg)を向くのでそこを原点として処理するためのギャップ

    // 移動範囲制限のための範囲設定
    [SerializeField] GameArea gameArea = default; // 画面範囲の元となるmask画像
    [SerializeField] float borderRatioV = 0.95f;  // 画面端に対する調整率(水平)
    [SerializeField] float borderRatioH = 0.95f;  // 画面端に対する調整率(垂直)
    Rect borderRect = new Rect();                 // 移動範囲用の矩形

    // 入力操作受け付け
    [Inject] IInputProvider input;                // DIで紐づけ
    [SerializeField] float moveSense = 1.0f;      // 移動量に対する実際の移動距離調整用感度

    [SerializeField] Text posInfo = default;


    /***** ReactivePropertyで監視させるものたち ****************************************************/
    // IReadOnlyReactivePropertyで公開してValueは変更できないようにする
    // ライフ
    [SerializeField] int defaultLife = 3;
    private ReactiveProperty<int> _lifeReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> LifeReactiveProperty { get { return _lifeReactiveProperty; } }

    // ボム数
    [SerializeField] int defaultBomb = 3;
    ReactiveProperty<int> _bombReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> BombReactiveProperty { get { return _bombReactiveProperty; } }


    /***** MonoBehaviourイベント処理 ****************************************************/
    void Start()
    {
        // 初期化
        // TODO:今はここで呼ぶが本来は読み込み完了のSignalを受けて呼ぶ
        OnSettingInfoLoaded();

        // 移動範囲設定
        SetMoveArea();

    }

    void Update()
    {
        // 操作入力を受け取ってもろもろ動作する
        //InputAction();

    }

    /***** Signal受信時処理 **************************************************/
    // 設定読み込み完了
    void OnSettingInfoLoaded()
    {
        // TODO:ライフとボム数は前ステージの値を引き継ぐ 1面は初期値
        _lifeReactiveProperty.Value = defaultLife;
        _bombReactiveProperty.Value = defaultBomb;
    }

    /***** Playe個別処理 ****************************************************/
    // 移動範囲設定
    void SetMoveArea()
    {
        // GameAreaを一定範囲狭めたものを移動範囲とする
        borderRect = gameArea.GetGameAreaRect();

        float borderShrinkV = (borderRect.xMax - borderRect.xMin) * (1.0f - borderRatioV);
        float borderShrinkH = (borderRect.yMax - borderRect.yMin) * (1.0f - borderRatioH);

        borderRect.xMin += borderShrinkV;
        borderRect.xMax -= borderShrinkV;
        borderRect.yMin += borderShrinkH;
        borderRect.yMax -= borderShrinkH;

        /*
        Debug.Log("left-bottom:(" + borderRect.xMin.ToString("f1") + ", " + borderRect.yMin.ToString("f1") + "), " + 
                    "right-top:("   + borderRect.xMax.ToString("f1") + ", " + borderRect.yMax.ToString("f1") + ")");
            */

        return;
    }

    // ショット操作
    public void Shot()
    {
        // Debug.Log("Shot from InputPresenter!!");
        return;
    }

    // 投げ操作
    public void Throw(float angle)
    {
        Debug.Log("Throw from InputPresenter!! " + angle + " deg");
        return;
    }

    // ボム操作
    public void Bomb()
    {
        Debug.Log("Bomb from InputPresenter!!");
        return;
    }

    // 移動
    public void MovePlayer(MoveInfo info)
    {
        Vector2 moveSpeed = info.MoveSpeed;
        Vector2 movePosition = info.MovePosition;
        Vector2 newPosition = Vector2.zero;

        // 座標変換用
        float distanceToCamera = Mathf.Abs(Camera.main.gameObject.transform.position.z);  // メインカメラとの距離を取得

        // 端末に応じて動かし方を変える
        // ・携帯端末: 入力(タッチ)移動速度に応じて現在位置から動く
        // ・上記以外: 入力(マウスなど)のポインタ位置に動く

        // 移動距離・移動速度なしの場合は処理不要とする
        if (Vector2.zero == moveSpeed || Vector2.zero == movePosition)
        {
            Debug.Log("not need to move.");
            return;
        }

        // 携帯端末の場合
        if (PlatformInfo.IsMobile())
        {
            // 移動量取得
            // 取得した移動速度に感度とTime.deltaTimeをかけ合わせて自機移動量を出す
            // 使用端末の画面の大きさによらず、一定距離動かしたらゲーム画面上で一定割合自機が動くことを想定
            Vector2 dist = moveSpeed * moveSense * Time.deltaTime;

            // 現在位置をスクリーン座標に変換して移動量を加算
            Vector3 positionS = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position) + dist;

            // ワールド座標のスケールに変換
            newPosition = Camera.main.ScreenToWorldPoint(positionS);

        }
        // 携帯端末以外の場合
        else
        {
            // 入力位置(スクリーン座標)をワールド座標へ変換
            newPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera));
            //Debug.Log(Input.mousePosition + "->" + newPosition);

        }

        // 画面から出ない範囲で自機移動
        if (borderRect.xMax < newPosition.x) newPosition.x = borderRect.xMax;
        if (borderRect.xMin > newPosition.x) newPosition.x = borderRect.xMin;
        if (borderRect.yMax < newPosition.y) newPosition.y = borderRect.yMax;
        if (borderRect.yMin > newPosition.y) newPosition.y = borderRect.yMin;

        transform.position = newPosition;

        // Debug用位置情報表示
        //Debug.Log("move dist:" + dist + ", new pos: " + newPos);
        DispPosInfo(movePosition, moveSpeed, newPosition);
    }

    // Debug用位置情報表示
    void DispPosInfo(Vector2 inputPosition, Vector2 inputSpeed, Vector2 pos)
    {
        // 文字列生成
        posInfo.text =
            "Position:" + inputPosition.x.ToString("f1") + ", " + inputPosition.y.ToString("f1") + "\n" +
            "Speed:" + inputSpeed.x.ToString("f1") + ", " + inputSpeed.y.ToString("f1") + "\n" +
            "Player:" + pos.x.ToString("f1") + ", " + pos.y.ToString("f1");

        // 文字列移動
        float sideOffset = 0.0f;  // 横方向にずらす距離
        float heightOffset = 50.0f;  // 縦方向にずらす距離
        Vector2 dispPos = RectTransformUtility.WorldToScreenPoint(Camera.main, pos) + new Vector2(sideOffset, heightOffset);

        // 画面からは出さない範囲で調整
        if (Screen.width < dispPos.x + posInfo.GetComponent<RectTransform>().rect.width / 2) dispPos.x = Screen.width - posInfo.GetComponent<RectTransform>().rect.width / 2;
        if (0.0f > dispPos.x - posInfo.GetComponent<RectTransform>().rect.width / 2) dispPos.x = posInfo.GetComponent<RectTransform>().rect.width / 2;
        if (Screen.height < dispPos.y + posInfo.GetComponent<RectTransform>().rect.height / 2) dispPos.y = Screen.height - posInfo.GetComponent<RectTransform>().rect.height / 2;
        if (0.0f > dispPos.y - posInfo.GetComponent<RectTransform>().rect.height / 2) dispPos.y = posInfo.GetComponent<RectTransform>().rect.height / 2;

        posInfo.transform.position = dispPos;
    }
}
