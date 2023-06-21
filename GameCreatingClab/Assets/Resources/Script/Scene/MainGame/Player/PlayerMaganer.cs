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
        idol, //静止中　壁に当たってる中等
        walk,
        jump,
        attack,
        jump_attack, //jump & attackで発動
        //ダッシュはjump+attackで可能
        pause,
    }
    [SerializeField, ReadOnly,Header("ゲーム内の現在のキャラステータス")]
    charaState state = charaState.Init;
    [SerializeField,ReadOnly]charaState stockState = charaState.NONE; //ポーズから戻る際、保持するステータス
    [SerializeField, ReadOnly] PlayerStatu nowStatus;

    private Rigidbody rb;
    //ジャンプ用
    private bool falling; //落下中フラグ
    [SerializeField, ReadOnly] float y = 0, y_vel;
    [SerializeField, Header("最大のジャンプパワー")] float y_vel_max = 35;  //初速度
    [SerializeField, Header("ジャンプ（up）の加算値")] float y_a1 = 4;
    [SerializeField, Header("ジャンプ（down）の加算値")] float y_a2 = 2; //ジャンプアップ中の時の、ダウン中の加速度
    //アタック
    [SerializeField] private GameObject bulletObj;
    private bool ButtelCreate_Once = false; //玉発射したかどうか
    private float stayTime = 0;//アニメがない内はこれでテキトーにすごす
    //ジャンプアタック
    private BulletController CreateBullet = null;

    public Transform plTrf => this.transform.parent;
    public Vector3 plPos_world => this.transform.position;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

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
            case charaState.jump_attack: Dojump_attack(delta); break;
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
        RandChecker.cInstance.changeRand(true);

        y = 0;
        CreateBullet = null;

        rb.isKinematic = false;
    }

    private void DoIdol(float delta)
    {
        state = charaState.walk;
        RandChecker.cInstance.changeRand(true);

        rb.isKinematic = false;
    }

    private void DoWalk(float delta)
    {
        //カメラとキャラを動かす
        RandChecker.cInstance.changeRand(true);
        rb.isKinematic = false;

        //カメラが動いてなければ動かす
        if (!CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.moving);

        //入力受付ここでやる
        switch (PlayerInputSystem.cInstance.IsType) 
        {
            case buttonType.Jump:
                //ジャンプする前の設定代入
                y_vel = y_vel_max;
                y = y_vel;
                falling = false;
                RandChecker.cInstance.changeRand(false);

                state = charaState.jump;
                break;
            case buttonType.Attack:
                //アタック前の設定代入
                stayTime = 0; //待ち時間設定
                ButtelCreate_Once = false;

                state = charaState.attack;
                break;

            default:
                break;
        }

    }
    private void DoJump(float delta)
    {
        //★時々貫通するのでそれ修正予定

        //カメラとキャラを動かす
        //カメラが動いてなければ動かす
        if (!CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.moving);
        rb.isKinematic = false;

        //アタックの入力が入ったらattackに切り替え
        if (PlayerInputSystem.cInstance.IsType == buttonType.Attack)
        {
            stayTime = 0;
            ButtelCreate_Once = false;

            //ジャンプの頂点を現在地とする
            y_vel = 0;

            falling = true;
            state = charaState.jump_attack;
        }

        //上昇
        //if (!RandChecker.cInstance.IsRanding) //地面についてないとき
        if(!falling)
        {
            //途中でボタンを離した、または頂点に来た時
            if (y_vel <= 0)
            {
                y_vel = 0;
                falling = true;
            }
            else
            {
                //ジャンプ
                y_vel += -y_a1;
                y = y_vel;

            }
        }
        //落下
        else
        {
            //着地したら入力状態リセット（InputManager->Init）
            //if (DevelopMode && RandMode_dev) //一旦これ
            if (RandChecker.cInstance.IsRanding)
            {
                //着地用に再設定
                y = 0;
                falling = false;

                state = charaState.walk;
                PlayerInputSystem.cInstance.ResetStates();
            }
            else
            {
                y_vel += y_a2;
                y = -y_vel;

            }
        }
        rb.velocity = new Vector2(0, y);

    }

    private void DoAttack(float delta)
    {
        rb.isKinematic = false;
        //キャラの前方から斜め上発射弾を一直線上に出す
        //カーソル向きの処理
        //カメラが動いてなければ動かす

        //発射モーション中は停止
        //CameraManager.cInstance.ChangeState(CameraState.stop);
        //モーション終了でwalkへ以降、入力状態もリセット

        if (CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.stop);

        if (!ButtelCreate_Once)
        {
            Transform Intantiate_Parent = this.transform.parent;
            GameObject bullet = Instantiate(bulletObj, Intantiate_Parent); //弾の生成

            ButtelCreate_Once = true;
            
        }
        else
        {
            if (stayTime <= 1.0f)
            {
                stayTime += delta;
            }
            //適当に待ったら
            //if (DevelopMode && AttackFin_dev) //一旦これ
            else
            {
                stayTime = 0;
                state = charaState.walk;
                PlayerInputSystem.cInstance.ResetStates();
            }
        }
    }

    private void Dojump_attack(float delta = 0)
    {
        rb.isKinematic = false;
        //バックジャンプ（ジャンプ×アタック）
        //ジャンプ→アタックの順しか受け付けない

        /*
        　アニメーションのイメージ　※ここ話し合いで決める
        　１．ジャンプする
        　２．アタック開始で、posYはそのままでposXを移動　この時入力状態はattackになる
          ３．アタックモーション終了で落下開始（アタックが入った時点でソコが頂点扱いになる）
        　４．着地でstate=walk & 入力状態リセット
         */



        if (!ButtelCreate_Once)
        {
            if (CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.stop);
            Transform Intantiate_Parent = this.transform.parent;
            GameObject bullet = Instantiate(bulletObj, Intantiate_Parent); //弾の生成
            CreateBullet = bullet.GetComponent<BulletController>();

            ButtelCreate_Once = true;

        }
        else
        {
            if (CreateBullet.IsInit) return; //団情報初期設定中は通らない

            //待ち時間
            if (stayTime <= 1.0f)
            {
                stayTime += delta;
            }
            
            //重力落下する
            //着地したら入力状態リセット（InputManager->Init）
            if (RandChecker.cInstance.IsRanding)
            {
                CameraManager.cInstance.ChangeState(CameraState.stop);
                y = 0;
            }
            else
            {
                //落下中
                if (CreateBullet.getMousePos.x >= Screen.width / 2) 
                    CameraManager.cInstance.ChangeState(CameraState.back_moving);
                else
                    CameraManager.cInstance.ChangeState(CameraState.dash_moving);
                y_vel += y_a2;
                y = -y_vel;

            }
            rb.velocity = new Vector2(0, y);

            //落下と待ち時間が終わったら終了
            if (stayTime > 1.0f && RandChecker.cInstance.IsRanding)
            {
                stayTime = 0;
                state = charaState.walk;
                PlayerInputSystem.cInstance.ResetStates();

                //着地用に再設定
                y = 0;
                falling = false;

                state = charaState.walk;
                PlayerInputSystem.cInstance.ResetStates();
            }   
        }

        /*
        //最終的にtype=attackになるなら、attackと同じ方法で止まるはず
        if (DevelopMode && AttackFin_dev) //一旦これ
        {
            state = charaState.walk;
            PlayerInputSystem.cInstance.ResetStates();

            if (DevelopMode && AttackFin_dev) AttackFin_dev = false;
        }
        */
    }

    private void DoPause(float delta = 0)
    {
        rb.isKinematic = true;
        //ポーズ中　ジャンプや諸々を0にする
        //→この方法でやるなら重力はプログラム操作になる？
        //　→★0621:重力はRighdBodyでやってみることに　ソレの設定を変更する形でやる…
        if (CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.stop);

        if (PlayerInputSystem.cInstance.IsType == buttonType.Unpause)
        //if (DevelopMode && Unpause_dev) //一旦これ
        {
            state = stockState; //保持したものに変更
            stockState = charaState.NONE;
            PlayerInputSystem.cInstance.Unpause_Stock();//ポーズ解除で入力状態を保持したものに変更
        }
    }

    #endregion
}
