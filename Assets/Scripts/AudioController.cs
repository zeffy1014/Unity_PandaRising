using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BGMリスト
public enum BGMList
{
    Title,              // タイトル画面
    Menu_Main,          // メニュー画面
    Game_Stage,         // プレー中BGM(ステージごとに異なる)
    Game_Boss,          // ボス戦中(ステージごとに異なる)

    BGM_TotalNumber

}

// SEリスト
public enum SEList
{
    // Player関連
    Player_Shot,        // 通常ショット
    Player_Throw,       // 投げ
    Player_Catch,       // 投げたもの回収・ブロック取得など
    Player_Laser,       // レーザー
    Player_Bomb,        // ボム
    Player_Damage,      // 被弾
    Player_Defeated,    // 撃沈
    Player_Launch,      // 出撃・クリア時の離脱

    // Enemy関連
    Enemy_Shot_01,
    Enemy_Shot_02,
    Enemy_Shot_03,
    Enemy_Damage,
    Enemy_Defeated_01,
    Enemy_Defeated_02,

    // その他
    House_Fall,         // 敵後逸で何か降ってくる音
    House_Damage,
    Stage_Clear,
    Stage_Warning,

    // System関連
    System_Select_01,
    System_Select_02,
    System_TransitionForward,
    System_TransitionBack,
    System_Reinforce,

    SE_TotalNumber

}


// BGM・SE再生用クラス
// 参考:https://qiita.com/2dgames_jp/items/20360f9797c7e8b166bc
public class AudioController
{
    // 固定値
    const int SE_CHANNEL = 4;  // SE再生チャンネル数

    /// サウンド種別
    public enum AudioType
    {
        BGM,
        SE,
    }

    // シングルトン
    static AudioController acInstance = null;
    public static AudioController Instance
    {
        get
        {
            return acInstance ?? (acInstance = new AudioController());
        }
    }

    // サウンド再生のためのゲームオブジェクト
    GameObject audioSourceHolder = null;
    // サウンドリソース
    AudioSource sourceBGM = null;   // BGM
    AudioSource sourceSE = null;    // SE (デフォルト)
    AudioSource[] sourceSEArray;    // SE (チャンネル)

    // BGMにアクセスするためのテーブル パフォーマンスのためkeyをBGMList(enum)ではなくintとする
    Dictionary<int,BGMData> tableBGM = new Dictionary<int, BGMData>();
    // SEにアクセスするためのテーブル パフォーマンスのためkeyをSEList(enum)ではなくintとする
    Dictionary<int, SEData> tableSE = new Dictionary<int, SEData>();

    /// 保持するデータ
    class BGMData
    {
        public AudioClip Clip;
        public BGMData(string res)
        {
            // AudioClipの取得
            Clip = Resources.Load(res) as AudioClip;
        }
    }
    class SEData
    {
        public AudioClip Clip;
        public SEData(string res)
        {
            // AudioClipの取得
            Clip = Resources.Load(res) as AudioClip;
        }
    }
    /***** 公開関数 ******************************************************************/
    // リソース読み込み(Resources 以下)
    public void LoadBGM(BGMList key, string resName)
    {
        if (tableBGM.ContainsKey((int)key))
        {
            // 登録済みだったら削除(上書きする)
            tableBGM.Remove((int)key);
        }
        tableBGM.Add((int)key, new BGMData(resName));

        return;
    }
    public void LoadSE(SEList key, string resName)
    {
        if (tableSE.ContainsKey((int)key))
        {
            // 登録済みだったら削除(上書きする)
            tableSE.Remove((int)key);
        }
        tableSE.Add((int)key, new SEData(resName));

        return;
    }

    // BGM再生(要事前登録)
    public bool PlayBGM(BGMList key, bool loop = true)
    {
        if (false == tableBGM.ContainsKey((int)key))
        {
            // 対応するキーがない場合は再生不可
            Debug.Log("PlayBGM key:" + key + " has not loaded...");
            return false;
        }

        // いったんBGMを止める
        StopBGM();

        // リソースの取得
        BGMData bgm = tableBGM[(int)key];

        // 再生
        AudioSource source = GetAudioSource(AudioType.BGM);
        source.loop = loop;
        source.clip = bgm.Clip;
        source.Play();

        return true;
    }

    /// BGM停止 TODO:フェードアウトもできるようにしたい
    public bool StopBGM()
    {
        GetAudioSource(AudioType.BGM).Stop();
        return true;
    }

    /// SE再生(要事前登録)
    public bool PlaySE(SEList key, int channel = -1)
    {
        if (tableSE.ContainsKey((int)key) == false)
        {
            // 対応するキーがない場合は再生不可
            Debug.Log("PlaySE key:" + key + " has not loaded...");
            return false;
        }

        // リソースの取得
        SEData se = tableSE[(int)key];

        if (0 <= channel && channel < SE_CHANNEL)
        {
            // チャンネル指定
            var source = GetAudioSource(AudioType.SE, channel);
            source.clip = se.Clip;
            source.Play();
        }
        else
        {
            // チャンネル指定なし
            var source = GetAudioSource(AudioType.SE);
            source.PlayOneShot(se.Clip);  // 重複ありで再生
        }

        return true;
    }

    /***** 非公開関数 ******************************************************************/
    // コンストラクタ
    private AudioController()
    {
        // チャンネル確保
        sourceSEArray = new AudioSource[SE_CHANNEL];
        // AudioSource生成しておく
        GetAudioSource(AudioType.BGM);
    }

    /// AudioSourceを取得する
    AudioSource GetAudioSource(AudioType type, int channel = -1)
    {
        // AudioSourceがまだ無ければつくる
        if(audioSourceHolder == null)
        {
            // GameObjectがなければ作る
            audioSourceHolder = new GameObject("AudioController");
            // 破棄しないようにする
            GameObject.DontDestroyOnLoad(audioSourceHolder);
            // AudioSourceを作成
            sourceBGM = audioSourceHolder.AddComponent<AudioSource>();
            sourceSE = audioSourceHolder.AddComponent<AudioSource>();
            for (int i = 0; i < SE_CHANNEL; i++)
            {
                sourceSEArray[i] = audioSourceHolder.AddComponent<AudioSource>();
            }
        }

        if(AudioType.BGM == type)
        {
            return sourceBGM;
        }
        else
        {
            // SE
            if(0 <= channel && channel < SE_CHANNEL)
            {
                // チャンネル指定あり
                return sourceSEArray[channel];
            }
            else
            {
                // チャンネル指定なし
                return sourceSE;
            }
        }
    }




}


