using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class YesNoDialog : MonoBehaviour
{
    private Text title = default;
    private Text description = default;
    private Button yesButton = default;
    private Button noButton = default;

    private static readonly string PREFAB_NAME = "YesNoDialogPrefab";
    private static GameObject prefab;

    public static YesNoDialog ShowDialog(
        string title,        // ダイアログ上部のタイトル
        string description,  // ダイアログ本文
        string yes = null,   // Yesボタンの記述(無ければデフォルトの「はい」)
        string no = null     // Noボタンの記述(無ければデフォルトの「いいえ」)
    )
    {
        if (null == prefab)
        {
            prefab = (GameObject)Resources.Load(PREFAB_NAME);
        }

        var instance = Instantiate(prefab);
        var handler = instance.GetComponent<YesNoDialog>();

        // 各種文字を設定
        // TODO:文字数に応じてサイズを変えたい
        handler.title.text = title;
        handler.description.text = description;

        if (!string.IsNullOrEmpty(yes))
        {
            handler.yesButton.GetComponentInChildren<Text>().text = yes;
        }
        handler.yesButton.onClick.AddListener(() => Destroy(handler.gameObject));

        if (!string.IsNullOrEmpty(no))
        {
            handler.noButton.GetComponentInChildren<Text>().text = no;
        }
        handler.noButton.onClick.AddListener(() => Destroy(handler.gameObject));

        return handler;
    }

    // Yesボタンの処理登録
    public void SetYesAction(UnityAction action)
    {
        this.yesButton.onClick.AddListener(action);
    }

    // Noボタンの処理登録
    public void SetNoAction(UnityAction action)
    {
        this.noButton.onClick.AddListener(action);
    }
}