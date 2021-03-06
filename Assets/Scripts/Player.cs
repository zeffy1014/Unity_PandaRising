﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Zenject;
using InputProvider;
using Bullet;
using DataBase;
using TMPro;

// Playerの魚の所持状態
public enum FishState
{
    Holding,   // 所持している
    Throwing,  // 投げている途中
    Lost       // 失った状態
}

// Playerの岩掴み関連状態
public enum BlockState
{
    EmptyHanded,    // 手ぶら状態(通常, Block入手可)
    Shot,           // ショット操作中(Block入手不可)
    Holding         // Block保持中
}

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
    [SerializeField] float moveSense = 1.0f;      // 移動量に対する実際の移動距離調整用感度

    [SerializeField] TextMeshProUGUI posInfo = default;

    // 弾生成用
    [Inject] BulletGenerator bulletGenerator;
    [SerializeField] float shotInterval = default; // 連射間隔(sec)
    float shotWait = default;                      // 次の弾が撃てるまでの待ち時間(sec)

    // 魚関連
    public float FishLifeTime { get; private set; } = default;   // 魚の生存時間(DBから取得)
    public float FishRevivalTime { get; private set; } = 3.0f;   // 魚の復活時間(固定値)
    float fishRevivalWait = default;
    float fishCatchTime = 0.5f;                                  // 魚を回収できるまでの待ち時間(発射時に回収されないように)
    float fishCatchWait = default;

    // 音関連
    [SerializeField] SEList shotSound = SEList.Player_Shot;
    [SerializeField] SEList throwSound = SEList.Player_Throw;
    [SerializeField] SEList catchSound = SEList.Player_Catch;
    [SerializeField] SEList damageSound = SEList.Player_Damage;
    [SerializeField] SEList defeatedSound = SEList.Player_Defeated;

    // 演出
    [SerializeField] GameObject damageEffect;

    // 被弾時処理用
    public bool BeingDamaged { get; private set; } = false;     // 被弾状態かどうか
    [SerializeField] float returnInterval = 3.0f;               // 被弾状態から戻るまでの時間(sec)    
    float returnWait = default;                                 // 被弾状態の経過時間(sec)
    float blinkSpan = 0.1f;                                     // 点滅周期(sec)

    /***** ReactivePropertyで監視させるものたち ****************************************************/
    // IReadOnlyReactivePropertyで公開してValueは変更できないようにする
    // ライフ　※シーンをまたいで引き継ぐ
    [SerializeField] static int defaultLife = 3;
    static ReactiveProperty<int> _lifeReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> LifeReactiveProperty { get { return _lifeReactiveProperty; } }

    // ボム数　※シーンをまたいで引き継ぐ
    [SerializeField] static int defaultBomb = 3;
    static ReactiveProperty<int> _bombReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> BombReactiveProperty { get { return _bombReactiveProperty; } }

    // 魚所持状態
    ReactiveProperty<FishState> _fishStateReactiveProperty = new ReactiveProperty<FishState>(FishState.Holding);
    public IReadOnlyReactiveProperty<FishState> FishStateReactiveProperty => _fishStateReactiveProperty;

    // 岩掴み関連状態
    ReactiveProperty<BlockState> _blockStateReactiveProperty = new ReactiveProperty<BlockState>(BlockState.EmptyHanded);
    public IReadOnlyReactiveProperty<BlockState> BlockStateReactiveProperty => _blockStateReactiveProperty;
    public BulletType blockType = default;      // 現在保持しているBlock弾の種別
    public GameObject block = default;          // 現在保持しているBlock弾のPrefab

    /***** MonoBehaviourイベント処理 ****************************************************/
    void Start()
    {
        // ライフ未設定の場合は初期化する(本来はシーンロード前にやっておく)
        if (0 == _lifeReactiveProperty.Value)
        {
            Debug.Log("Player Life is Zero -> Init Data");
            InitStaticData();
        }

        // 設定読み込み
        bool loadResult = LoadData();
        if (!loadResult)
        {
            // TODO:読み込み失敗したらエラー通知してメインメニューに戻る？
            Debug.Log("Player load data failed...");
        }

        // 移動範囲設定
        SetMoveArea();

    }

    void Update()
    {
        // 連射待ち時間を減らす
        shotWait = (shotWait > Time.deltaTime)
            ? shotWait - Time.deltaTime
            : 0.0f;

        // 魚関連
        if (FishState.Lost == FishStateReactiveProperty.Value)
        {
            fishRevivalWait -= Time.deltaTime;
            if (0.0f >= fishRevivalWait)
            {
                // 魚復活
                _fishStateReactiveProperty.Value = FishState.Holding;
            }
        }
        else if (FishState.Throwing == FishStateReactiveProperty.Value)
        {
            // 回収できる状態になるための待ち時間を減らす
            fishCatchWait -= Time.deltaTime;
        }
    }

    /***** Zenject Signal受信 ****************************************************/
    // 操作
    public void OnFishLost(FishLostSignal signal)
    {
        Debug.Log("Fish has lost...");
        _fishStateReactiveProperty.Value = FishState.Lost;
        fishRevivalWait = FishRevivalTime;
        return;
    }

    /***** Collider2Dイベント処理 ****************************************************/
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 魚を回収
        if ("Bullet" == other.gameObject.tag && BulletType.Player_Fish == other.GetComponent<Bullet.Bullet>().Type)
        {
            // 一定時間経過していたら回収
            if (0.0f >= fishCatchWait)
            {
                Destroy(other.gameObject);
                _fishStateReactiveProperty.Value = FishState.Holding;

                // キャッチ音
                AudioController.Instance.PlaySE(catchSound);

            }
        }
        // 敵または敵弾と接触
        if ("Enemy" == other.gameObject.tag || "Bullet_Enemy" == other.gameObject.tag)
        {
            // 点滅無敵でなければ被弾
            if (!BeingDamaged)
            {
                // 被弾エフェクトと音
                Instantiate(damageEffect, transform.position, Quaternion.identity);
                AudioController.Instance.PlaySE(damageSound);

                _lifeReactiveProperty.Value--;
                if (0 == _lifeReactiveProperty.Value)
                {
                    // TODO:ライフゼロの処理 Signal発行など
                }
                else
                {
                    // TODO:被弾のSignal発行(コンボ切れなどに使う)

                    // しばらく点滅無敵状態
                    BeingDamaged = true;
                    StartCoroutine(DamageBlink());
                }
            }

        }

    }

    /***** Playe個別処理 ****************************************************/
    // データ初期化(新規ゲーム開始時 シーンロード前に呼ぶこと！)
    static public void InitStaticData()
    {
        Debug.Log("Player InitStaticData");

        // デフォルトのライフとボム数設定
        _lifeReactiveProperty.Value = defaultLife;
        _bombReactiveProperty.Value = defaultBomb;
    }

    // 各種設定・情報読み込み
    bool LoadData()
    {
        // プレーデータと各種強化テーブル取得
        UserData userData = DataLibrarian.Instance.GetUserData();
        ReinforcementTableInfo rtInfo = DataLibrarian.Instance.GetReinforcementTableInfo();

        // 読み込み成否確認
        if (null == userData || null == rtInfo)
        {
            // 駄目だった
            Debug.Log("Player LoadData failed...");
            return false;
        }

        // 各種強化レベルと対応パラメータから設定
        // 弾の連射間隔設定 + 待ち時間初期化
        shotInterval= rtInfo.GetShotRapidity(userData.GetLevel(ReinforceTarget.ShotRapidity));
        shotWait = 0.0f;
        // 魚の生存時間設定 復活時間は固定値なので設定不要
        FishLifeTime = rtInfo.GetFishToughness(userData.GetLevel(ReinforceTarget.FishToughness));

        return true;
    }

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
    // 引数 true:操作中, false:操作やめ中
    public void Shot(bool action)
    {
        if (action)
        {
            // 手ぶら状態ならショット中にする
            if (BlockState.EmptyHanded == BlockStateReactiveProperty.Value) _blockStateReactiveProperty.Value = BlockState.Shot;

            // 連射間隔が既に経過していれば発射
            if (0.0f >= shotWait)
            {
                // 発射位置と向き
                Vector3 genPos = transform.position;
                genPos.y += 1.0f;
                Vector3 genRot = transform.rotation.eulerAngles;

                // Debug.Log("Shot from InputPresenter!!");
                bulletGenerator.ShotBullet(genPos, genRot, BulletType.Player_Mikan, GetNowAngle());

                shotWait = shotInterval;

                // 発射音
                AudioController.Instance.PlaySE(shotSound);
            }
        }
        else
        {
            // ショット中なら手ぶら状態にする
            if (BlockState.Shot == BlockStateReactiveProperty.Value) _blockStateReactiveProperty.Value = BlockState.EmptyHanded;

        }

        return;
    }

    // 投げ操作
    public void Throw(float angle)
    {
        Debug.Log("Throw from InputPresenter!! " + angle + " deg");

        // 位置と向きを指定
        Vector3 genPos = transform.position;
        genPos.y += 1.0f;
        Vector3 genRot = transform.rotation.eulerAngles;

        // angleが負の場合は正面に投げる
        if (0.0f > angle) angle = GetNowAngle();

        // Block保持中はBlockを投げる
        if (BlockState.Holding == BlockStateReactiveProperty.Value)
        {
            bulletGenerator.ShotBullet(genPos, genRot, blockType, angle);
            _blockStateReactiveProperty.Value = BlockState.EmptyHanded;

            // 投げた音
            AudioController.Instance.PlaySE(throwSound);

        }
        // Blockなしで魚があったら魚を投げる
        else if (FishState.Holding == FishStateReactiveProperty.Value)
        {
            bulletGenerator.ShotBullet(genPos, genRot, BulletType.Player_Fish, angle);
            _fishStateReactiveProperty.Value = FishState.Throwing;
            fishCatchWait = fishCatchTime;

            // 投げた音
            AudioController.Instance.PlaySE(throwSound);
        }
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

    // 現在の角度取得(頭が向いている方向)
    protected float GetNowAngle()
    {
        return ANGLE_GAP + transform.eulerAngles.z;
    }

    // 被弾時の点滅処理
    IEnumerator DamageBlink()
    {
        returnWait = 0.0f;

        // 一定期間処理を続ける
        while(BeingDamaged)
        {
            yield return null;

            returnWait += Time.deltaTime;
            Color color = this.gameObject.GetComponent<SpriteRenderer>().material.color;
            // 経過時間に2*PIをかけると1秒で1回点滅する→更に点滅周期で割る
            color.a = Mathf.Sin((returnWait * (2 * Mathf.PI)) / blinkSpan ) * 0.5f + 0.5f;
            this.gameObject.GetComponent<SpriteRenderer>().material.color = color;

            if (returnInterval < returnWait)
            {
                // 無敵時間終了 Alphaも通常に戻る
                BeingDamaged = false;
                color.a = 1.0f;
                this.gameObject.GetComponent<SpriteRenderer>().material.color = color;
            }
        }
    }

    // Block接触
    // 戻り値 true:掴み状態遷移した, false:遷移しなかった
    public bool OnTouchBlock(BulletType type)
    {
        bool ret = false;

        // 手ぶらだったら岩掴み状態になる
        if (BlockState.EmptyHanded == BlockStateReactiveProperty.Value)
        {
            // 取得したBlock弾の情報を保持
            blockType = type;
            block = (GameObject)Resources.Load(DataLibrarian.Instance.GetBulletPrefabPath(type));

            _blockStateReactiveProperty.Value = BlockState.Holding;

            // キャッチ音
            AudioController.Instance.PlaySE(catchSound);

            ret = true;
        }

        return ret;
    }
}
