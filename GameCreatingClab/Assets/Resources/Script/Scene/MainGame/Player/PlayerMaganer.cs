using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CameraState = CameraManager.CameraState;
using buttonType = PlayerInputSystem.buttonType;

[System.Serializable]
public class PlayerStatu
{
    [Header("歩く速さ")]
    public float walkSpeed = 1.0f;
    [Header("助走の有/無")]
    public bool IsApproachRun;
    [Header("ジャンプ力")]
    public float jumpPower = 5.0f;
    [Header("重力")]
    public float gravityPower = 1.0f;
    [Header("発射弾の速さ")]
    public float bulletSpeed = 2.0f;

    public void Init(PlayerStatu config)
    {
        walkSpeed = 0;
        IsApproachRun = config.IsApproachRun;
        jumpPower = 0;
        gravityPower = config.gravityPower;
        bulletSpeed = config.bulletSpeed;
    }
}

public class PlayerMaganer : MonoBehaviour
{
    #region　外部参照
    public static PlayerMaganer cInstance = null;

    private void Awake()
    {
        if (cInstance == null)
        {
            cInstance = this;
        }
    }
    #endregion

    [SerializeField, Header("キャラのステータス値の設定")]
    PlayerStatu configStatus;
    enum charaState
    {
        NONE = 0,
        Init,
        //---
        idol,
        walk,
        jump,
        attack,
        backjump, //jump & attackで発動
        //ダッシュはjump+attackで可能
        pause,
    }
    [SerializeField, ReadOnly,Header("ゲーム内の現在のキャラステータス")]
    charaState state = charaState.Init;
    [SerializeField, ReadOnly] charaState stockState = charaState.NONE; //ポーズから戻る際、保持するステータス
    [SerializeField, ReadOnly]
    PlayerStatu nowStatus;

    [SerializeField] private float JumoPower = 30f;
    [SerializeField,ReadOnly] private float nowJumoPower;

    [SerializeField, Header("★開発用フラグ")] bool DevelopMode;
    [SerializeField, Header("★開発用\nジャンプ着地判定")] bool RandMode_dev;
    [SerializeField, Header("★開発用\nアタック終了判定")] bool AttackFin_dev;
    [SerializeField, Header("★開発用\nポーズ解除判定")] bool Unpause_dev;

    public Transform plTrf => this.transform.parent;

    private void Start()
    {
        state = charaState.Init;
        stockState = charaState.NONE;
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        switch (state)
        {
            case charaState.Init:    DoInit();        break;
            case charaState.idol:    DoIdol(delta);   break;
            case charaState.walk:    DoWalk(delta);   break;
            case charaState.jump:    DoJump(delta);   break;
            case charaState.attack:  DoAttack(delta); break;
            case charaState.backjump: DoBackJump(delta); break;
            case charaState.pause:   DoPause(delta);  break;
        }

        //初期化状態以外はポーズが効く
        if(state != charaState.Init)
        {
            //ポーズの入力があれば、ポーズ状態になる
            if (PlayerInputSystem.cInstance.IsType == buttonType.Pause)
            {
                if (stockState == charaState.NONE)
                {
                    stockState = state;//ポーズに入るときは、今の自分のステータスをストックに保持しておく
                    state = charaState.pause;
                    //現在のjumppowerとかの保持は無くてよいハズ
                }
            }
        }
    }

//---
    #region メイン関数
    private void DoInit(float delta = 0f)
    {
        nowStatus.Init(configStatus);
        state = charaState.idol;
        stockState = charaState.NONE;
    }

    private void DoIdol(float delta)
    {
        //とりますぐwalkにパスする
        state = charaState.walk;
    }

    private void DoWalk(float delta)
    {
        //カメラとキャラを動かす

        //カメラが動いてなければ動かす
        if (!CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.moving);

        //入力受付ここでやる
        switch (PlayerInputSystem.cInstance.IsType) 
        {
            case buttonType.Jump:
                state = charaState.jump;
                break;
            case buttonType.Attack:
                state = charaState.attack;
                break;

            default:
                break;
        }

    }
    private void DoJump(float delta)
    {
        //カメラとキャラを動かす
        //カメラが動いてなければ動かす
        if (!CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.moving);

        if(PlayerInputSystem.cInstance.IsType == buttonType.Attack) state = charaState.attack;

        //着地したら入力状態リセット（InputManager->Init）
        if (DevelopMode && RandMode_dev) //一旦これ
        {
            state = charaState.walk;
            PlayerInputSystem.cInstance.ResetStates();

            if (DevelopMode && RandMode_dev) RandMode_dev = false;
        }
    }

    private void DoAttack(float delta)
    {
        //キャラの前方から斜め上発射弾を一直線上に出す
        //カーソル向きの処理
        //カメラが動いてなければ動かす

        //発射モーション中は停止
        //CameraManager.cInstance.ChangeState(CameraState.stop);
        //モーション終了でwalkへ以降、入力状態もリセット

        if (CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.stop);
        if (DevelopMode && AttackFin_dev) //一旦これ
        {
            state = charaState.walk;
            PlayerInputSystem.cInstance.ResetStates();

            if (DevelopMode && AttackFin_dev) AttackFin_dev = false;
        }
    }

    private void DoBackJump(float delta = 0)
    {
        //バックジャンプ（ジャンプ×アタック）
        //ジャンプ→アタックの順しか受け付けない

        /*
        　アニメーションのイメージ　※ここ話し合いで決める
        　１．ジャンプする
        　２．アタック開始で、posYはそのままでposXを移動　この時入力状態はattackになる
          ３．アタックモーション終了で落下開始（アタックが入った時点でソコが頂点扱いになる）
        　４．着地でstate=walk & 入力状態リセット
         */

        //最終的にtype=attackになるなら、attackと同じ方法で止まるはず
        if (DevelopMode && AttackFin_dev) //一旦これ
        {
            state = charaState.walk;
            PlayerInputSystem.cInstance.ResetStates();

            if (DevelopMode && AttackFin_dev) AttackFin_dev = false;
        }

    }

    private void DoPause(float delta = 0)
    {
        //ポーズ中　ジャンプや諸々を0にする
        //→この方法でやるなら重力はプログラム操作になる？
        //　→★0621:重力はRighdBodyでやってみることに　ソレの設定を変更する形でやる…
        if (CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.stop);
        if (DevelopMode && Unpause_dev) //一旦これ
        {
            state = stockState; //保持したものに変更
            stockState = charaState.NONE;
            PlayerInputSystem.cInstance.Unpause_Stock();//ポーズ解除で入力状態を保持したものに変更

            if (DevelopMode && Unpause_dev) Unpause_dev = false;
        }
    }

    #endregion
//---
}
