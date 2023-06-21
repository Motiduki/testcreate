using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation; //BezierPathCreatorの使用　参考サイト：https://tech.pjin.jp/blog/2021/09/04/unity-rungame-05

public class CameraManager : MonoBehaviour
{
    #region　外部参照
    public static CameraManager cInstance = null;

    private void Awake()
    {
        if (cInstance == null)
        {
            cInstance = this;
        }
    }
    #endregion

    public enum CameraState
    {
        Init = 0,
        stop,
        moving,
        dash_moving,
        back_moving,
        
        none_pause, //stopとは異なり、このモードではすぐさま停止する
    }

    [SerializeField, ReadOnly] CameraState state = CameraState.Init;
    private PlayerMaganer plMng;
    [SerializeField, Header("カメラの移動速度")]private float moveSpeed = 6f; //設定する速度 ※★0621:一旦初速度とかは考えない（止・動だけ）今後実装予定
    [SerializeField, ReadOnly] float nowSpeed = 6f; //現在のゲーム内の速度
    //初速度とか色々やる

    //PathCreator関連
    /*[SerializeField, Header("PathCreatorの代入\nNullで使用無効")]*/ PathCreator pathCreator;
    /*[SerializeField ,ReadOnly ,Header("現在の移動距離")] */float moveDistance;
   /* [SerializeField, ReadOnly ,Header("最終地点")] */Vector3 endPos;
    private bool IsUsingBezier => (pathCreator != null); //PathCreatorを使用しているか

    //StageManagerを回転させる
    [SerializeField] Transform StageParentObj;
    private bool IsStageParRotate => (StageParentObj != null); //ステージの親オブジェを回転させる形で進めるか

    //---
    [SerializeField,Header("★開発用フラグ")]
    private bool DevelopMode = false;
    [SerializeField, Header("★カメラの停止/動作\nステ変関数より優先して止・動する")]
    private bool MovingCamera_Dev = false;

    //カメラが動作中かどうか
    public bool IsMovingCamera => DevelopMode ? (MovingCamera_Dev) : (state == CameraState.moving || state == CameraState.back_moving || state == CameraState.dash_moving);
    //カメラのステータス変更
    public void ChangeState(CameraState _state = CameraState.none_pause) => state = _state;


    private void Start()
    {
        //代入とかいろいろ
        plMng = PlayerMaganer.cInstance;
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        float mul_speed = 2.5f;
        switch (state) 
        {
            case CameraState.Init:   DoInit();        break;
            case CameraState.stop:   DoStop();        break;
            case CameraState.moving: DoMoving(delta,moveSpeed); break;
            case CameraState.dash_moving: DoMoving(delta, moveSpeed * mul_speed); break;
            case CameraState.back_moving: DoMoving(delta, -(moveSpeed * mul_speed)); break;
            case CameraState.none_pause:  DoPause();  break;
        }
    }

//---
    #region メイン関数
    private void DoInit(float delta = 0f)
    {
        //初期化
        if (IsUsingBezier)
        {
            endPos = pathCreator.path.GetPoint(pathCreator.path.NumPoints - 1);
        }
    }

    private void DoStop(float delta = 0)
    {
        //動作を止める
        if (IsUsingBezier)
        {
            DoPathCreatorCode(0);
        }

        /*
        if ((DevelopMode && MovingCamera_Dev)
        {
            state = CameraState.moving;
        }
        */
    }

    private void DoMoving(float delta,float speed)
    {
        //カメラとキャラを動かす
        if (IsUsingBezier)
        {
            DoPathCreatorCode(delta); //なんとかPahtCreatorのとき
        }
        else if (IsStageParRotate)
        {
            DoStageParRotateCode(delta, speed);
        }

        /*
        if ((DevelopMode && !MovingCamera_Dev))
        {
            state = CameraState.stop;
        }
        */
    }
    private void DoPause(float delta = 0)
    {
        //ポーズ中　ジャンプや諸々を0にする
        //→この方法でやるなら重力はプログラム操作になる？
    }

    #endregion

//---
    private void DoPathCreatorCode(float delta)
    {
        moveDistance += moveSpeed * delta;
        //EndOfPathInstruction(enum)…最終地点に来たら Loop、Reverse、Stop
        plMng.plTrf.position = pathCreator.path.GetPointAtDistance(moveDistance, EndOfPathInstruction.Loop);
        plMng.plTrf.rotation = pathCreator.path.GetRotationAtDistance(moveDistance, EndOfPathInstruction.Loop); 
    }

    private void DoStageParRotateCode(float delta,float speed)
    {
        //float speed = posi ? moveSpeed : -(moveSpeed * 2f);
        nowSpeed += speed * delta;

        StageParentObj.rotation = Quaternion.Euler(0, nowSpeed, 0);
    }

}
