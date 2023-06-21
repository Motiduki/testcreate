using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation; //BezierPathCreator�̎g�p�@�Q�l�T�C�g�Fhttps://tech.pjin.jp/blog/2021/09/04/unity-rungame-05

public class CameraManager : MonoBehaviour
{
    #region�@�O���Q��
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
        
        none_pause, //stop�Ƃ͈قȂ�A���̃��[�h�ł͂������ܒ�~����
    }

    [SerializeField, ReadOnly] CameraState state = CameraState.Init;
    private PlayerMaganer plMng;
    [SerializeField, Header("�J�����̈ړ����x")]private float moveSpeed = 6f; //�ݒ肷�鑬�x ����0621:��U�����x�Ƃ��͍l���Ȃ��i�~�E�������j��������\��
    [SerializeField, ReadOnly] float nowSpeed = 6f; //���݂̃Q�[�����̑��x
    //�����x�Ƃ��F�X���

    //PathCreator�֘A
    /*[SerializeField, Header("PathCreator�̑��\nNull�Ŏg�p����")]*/ PathCreator pathCreator;
    /*[SerializeField ,ReadOnly ,Header("���݂̈ړ�����")] */float moveDistance;
   /* [SerializeField, ReadOnly ,Header("�ŏI�n�_")] */Vector3 endPos;
    private bool IsUsingBezier => (pathCreator != null); //PathCreator���g�p���Ă��邩

    //StageManager����]������
    [SerializeField] Transform StageParentObj;
    private bool IsStageParRotate => (StageParentObj != null); //�X�e�[�W�̐e�I�u�W�F����]������`�Ői�߂邩

    //---
    [SerializeField,Header("���J���p�t���O")]
    private bool DevelopMode = false;
    [SerializeField, Header("���J�����̒�~/����\n�X�e�ϊ֐����D�悵�Ď~�E������")]
    private bool MovingCamera_Dev = false;

    //�J���������쒆���ǂ���
    public bool IsMovingCamera => DevelopMode ? (MovingCamera_Dev) : (state == CameraState.moving || state == CameraState.back_moving || state == CameraState.dash_moving);
    //�J�����̃X�e�[�^�X�ύX
    public void ChangeState(CameraState _state = CameraState.none_pause) => state = _state;


    private void Start()
    {
        //����Ƃ����낢��
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
    #region ���C���֐�
    private void DoInit(float delta = 0f)
    {
        //������
        if (IsUsingBezier)
        {
            endPos = pathCreator.path.GetPoint(pathCreator.path.NumPoints - 1);
        }
    }

    private void DoStop(float delta = 0)
    {
        //������~�߂�
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
        //�J�����ƃL�����𓮂���
        if (IsUsingBezier)
        {
            DoPathCreatorCode(delta); //�Ȃ�Ƃ�PahtCreator�̂Ƃ�
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
        //�|�[�Y���@�W�����v�⏔�X��0�ɂ���
        //�����̕��@�ł��Ȃ�d�͂̓v���O��������ɂȂ�H
    }

    #endregion

//---
    private void DoPathCreatorCode(float delta)
    {
        moveDistance += moveSpeed * delta;
        //EndOfPathInstruction(enum)�c�ŏI�n�_�ɗ����� Loop�AReverse�AStop
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
