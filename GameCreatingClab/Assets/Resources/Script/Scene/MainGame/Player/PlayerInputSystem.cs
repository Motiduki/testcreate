using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(PlayerMaganer))]
public class PlayerInputSystem : MonoBehaviour
{
    #region 外部参照
    public static PlayerInputSystem cInstance = null;

    private void Awake()
    {
        if (cInstance == null)
        {
            cInstance = this;
        }
    }
    #endregion

    /*
     * 操作方法
    ・カーソル：照準操作（ﾊｲﾃﾞﾝﾃﾞｨｰﾌﾟみたいなイメージ）
    ・マウス左：アタック
    ・スペースキーorホイール上：ジャンプ
    ・ESC：ポーズ 
     */
    enum buttonState
    {
        Stundby,
        Pushing, //押下した
        //Pushing_hold, //長押し中 いらないかも？いるかも？？
        //---↓特殊ステータス
        Jump_Pushing, //ジャンプ中はダッシュのみ受け付ける
    }
    public enum buttonType
    {
        NONE = 0,
        //とりあえずボタン操作にする
        Jump,
        Dash,
        Attack,
        Pause,
    }
    [SerializeField, ReadOnly] buttonState state = buttonState.Stundby;
    [SerializeField, ReadOnly] buttonType type = buttonType.NONE;
    buttonType stocktype = buttonType.NONE;

    public buttonType IsType => type;
    public bool IsStundby() => state == buttonState.Stundby;
    public void ResetStates() //ステートの初期化
    {
        state = buttonState.Stundby;
        type = buttonType.NONE;
    }
    public void Unpause_Stock()
    {
        switch (stocktype)
        {
            case buttonType.NONE:
                state = buttonState.Stundby;
                break;
            case buttonType.Jump:
                state = buttonState.Jump_Pushing;
                break;
            default:
                state = buttonState.Pushing;
                break;
        }
        type = stocktype;

        stocktype = buttonType.NONE;

    }

    private void Start()
    {
        ResetStates();
    }
    private void Update()
    {
        //Debug.Log("state:"+state +"_type:"+type + "_stocktype:"+stocktype);
        float delta = Time.deltaTime;
        switch (state)
        {
            case buttonState.Stundby:
                KeyDown_Keyboard(buttonType.Jump, KeyCode.W);
                KeyDown_Keyboard(buttonType.Dash, KeyCode.D);
                KeyDown_Keyboard(buttonType.Attack, KeyCode.A);
                break;
            case buttonState.Jump_Pushing: //ジャンプ中はアタックだけ受け付ける
                KeyDown_Keyboard(buttonType.Attack, KeyCode.A);
                break;
            case buttonState.Pushing:
                break;
        }
        KeyDown_Keyboard(buttonType.Pause, KeyCode.Escape);
    }

    //---
    private void KeyDown_Keyboard(buttonType _type, KeyCode _code)
    {
        if (Input.GetKeyDown(_code))
        {
            //ポーズに入る前は状態ストック
            if (_type == buttonType.Pause) stocktype = type;
            ChangeType(_type);
        }
    }
    //ポーズ解除あとの保持情報反映
    private void ChangeType(buttonType _type)
    {
        type = _type;
        //プレイヤーのstate状態も変更する

        switch (_type)
        {
            case buttonType.Jump:
                state = buttonState.Jump_Pushing;
                break;
            default:
                state = buttonState.Pushing;
                break;
        }
    }

}
