using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    //発射
    #region enum
    enum state
    {
        Init = 0, //初期化
        Moving,
        Finish
    }
    enum pos_type
    {
        start = 0,
        end,
        MAX
    }
    #endregion

    [SerializeField, ReadOnly] private state State = state.Init;
    [SerializeField, ReadOnly] private Vector3[] Position = new Vector3[(int)pos_type.MAX];
    [SerializeField] private float MoveSpeed = 3.0f;
    private Vector3 mousePos;

    public bool IsInit => State == state.Init;
    public Vector3 getMousePos => mousePos;

    private void Start()
    {
        
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        switch (State)
        {
            case state.Init: DoInit(delta); break;
            case state.Moving: DoMoving(delta); break;
            case state.Finish: DoFinish(delta); break;
        }
    }

    #region メイン関数
    private void DoInit(float _delta)
    {
        //カーソルの場所
        mousePos = Input.mousePosition;
        //mousePos.z = PlayerMaganer.cInstance.plPos_world.z;
        mousePos.x = Mathf.Clamp(mousePos.x, 0.0f, Screen.width);
        mousePos.y = Mathf.Clamp(mousePos.y, 0.0f, Screen.height);
        mousePos.z = 10;
        Vector3 CursolPos = Camera.main.ScreenToWorldPoint(mousePos);
        
        //それぞれの値をセット
        Position[(int)pos_type.start] = PlayerMaganer.cInstance.plPos_world;
        Position[(int)pos_type.end] = CursolPos;

        //自分の初期位置・角度のセット
        transform.position = Position[(int)pos_type.start];
        Vector3 diff = (Position[(int)pos_type.end] - transform.position);
        transform.rotation = Quaternion.FromToRotation(Vector3.right, diff);

        //ステート変更
        State = state.Moving;
    }

    private void DoMoving(float _delta)
    {
        //Debug.Log(transform.position+"_"+ Position[(int)pos_type.end]);
        //現在地～目的地の距離が0.5f以下になったら到着判定
        if (Vector2.Distance(transform.position, Position[(int)pos_type.end]) <= 0.5f)
        {
            //ステート変更
            State = state.Finish;
        }
        //未到着ならlerpで動かす
        else
        {
            float delta = PlayerInputSystem.cInstance.IsPausing() ? 0 : _delta;
            float speed = (MoveSpeed) * delta;
            transform.position = new Vector3
                (
                    Mathf.Lerp(transform.position.x, Position[(int)pos_type.end].x, speed),
                    Mathf.Lerp(transform.position.y, Position[(int)pos_type.end].y, speed),
                    PlayerMaganer.cInstance.plPos_world.z
                );
        }
    }

    private void DoFinish(float _delta)
    {
        //終わりを告げる…
        Destroy(this.gameObject);
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.tag);
        if(other.gameObject.tag == "floor" && State == state.Moving)
        {
            //ステート変更
            State = state.Finish;
        }
    }
}
