using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CameraState = CameraManager.CameraState;
using buttonType = PlayerInputSystem.buttonType;

[System.Serializable]
public class PlayerStatu
{
    [Header("��������")]
    public float walkSpeed = 1.0f;
    [Header("�����̗L/��")]
    public bool IsApproachRun;
    [Header("�W�����v��")]
    public float jumpPower = 5.0f;
    [Header("�d��")]
    public float gravityPower = 1.0f;
    [Header("���˒e�̑���")]
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
    #region�@�O���Q��
    public static PlayerMaganer cInstance = null;

    private void Awake()
    {
        if (cInstance == null)
        {
            cInstance = this;
        }
    }
    #endregion

    [SerializeField, Header("�L�����̃X�e�[�^�X�l�̐ݒ�")]
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
        backjump, //jump & attack�Ŕ���
        //�_�b�V����jump+attack�ŉ\
        pause,
    }
    [SerializeField, ReadOnly,Header("�Q�[�����̌��݂̃L�����X�e�[�^�X")]
    charaState state = charaState.Init;
    [SerializeField, ReadOnly] charaState stockState = charaState.NONE; //�|�[�Y����߂�ہA�ێ�����X�e�[�^�X
    [SerializeField, ReadOnly]
    PlayerStatu nowStatus;

    [SerializeField] private float JumoPower = 30f;
    [SerializeField,ReadOnly] private float nowJumoPower;

    [SerializeField, Header("���J���p�t���O")] bool DevelopMode;
    [SerializeField, Header("���J���p\n�W�����v���n����")] bool RandMode_dev;
    [SerializeField, Header("���J���p\n�A�^�b�N�I������")] bool AttackFin_dev;
    [SerializeField, Header("���J���p\n�|�[�Y��������")] bool Unpause_dev;

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

        //��������ԈȊO�̓|�[�Y������
        if(state != charaState.Init)
        {
            //�|�[�Y�̓��͂�����΁A�|�[�Y��ԂɂȂ�
            if (PlayerInputSystem.cInstance.IsType == buttonType.Pause)
            {
                if (stockState == charaState.NONE)
                {
                    stockState = state;//�|�[�Y�ɓ���Ƃ��́A���̎����̃X�e�[�^�X���X�g�b�N�ɕێ����Ă���
                    state = charaState.pause;
                    //���݂�jumppower�Ƃ��̕ێ��͖����Ă悢�n�Y
                }
            }
        }
    }

//---
    #region ���C���֐�
    private void DoInit(float delta = 0f)
    {
        nowStatus.Init(configStatus);
        state = charaState.idol;
        stockState = charaState.NONE;
    }

    private void DoIdol(float delta)
    {
        //�Ƃ�܂���walk�Ƀp�X����
        state = charaState.walk;
    }

    private void DoWalk(float delta)
    {
        //�J�����ƃL�����𓮂���

        //�J�����������ĂȂ���Γ�����
        if (!CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.moving);

        //���͎�t�����ł��
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
        //�J�����ƃL�����𓮂���
        //�J�����������ĂȂ���Γ�����
        if (!CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.moving);

        if(PlayerInputSystem.cInstance.IsType == buttonType.Attack) state = charaState.attack;

        //���n��������͏�ԃ��Z�b�g�iInputManager->Init�j
        if (DevelopMode && RandMode_dev) //��U����
        {
            state = charaState.walk;
            PlayerInputSystem.cInstance.ResetStates();

            if (DevelopMode && RandMode_dev) RandMode_dev = false;
        }
    }

    private void DoAttack(float delta)
    {
        //�L�����̑O������΂ߏ㔭�˒e���꒼����ɏo��
        //�J�[�\�������̏���
        //�J�����������ĂȂ���Γ�����

        //���˃��[�V�������͒�~
        //CameraManager.cInstance.ChangeState(CameraState.stop);
        //���[�V�����I����walk�ֈȍ~�A���͏�Ԃ����Z�b�g

        if (CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.stop);
        if (DevelopMode && AttackFin_dev) //��U����
        {
            state = charaState.walk;
            PlayerInputSystem.cInstance.ResetStates();

            if (DevelopMode && AttackFin_dev) AttackFin_dev = false;
        }
    }

    private void DoBackJump(float delta = 0)
    {
        //�o�b�N�W�����v�i�W�����v�~�A�^�b�N�j
        //�W�����v���A�^�b�N�̏������󂯕t���Ȃ�

        /*
        �@�A�j���[�V�����̃C���[�W�@�������b�������Ō��߂�
        �@�P�D�W�����v����
        �@�Q�D�A�^�b�N�J�n�ŁAposY�͂��̂܂܂�posX���ړ��@���̎����͏�Ԃ�attack�ɂȂ�
          �R�D�A�^�b�N���[�V�����I���ŗ����J�n�i�A�^�b�N�����������_�Ń\�R�����_�����ɂȂ�j
        �@�S�D���n��state=walk & ���͏�ԃ��Z�b�g
         */

        //�ŏI�I��type=attack�ɂȂ�Ȃ�Aattack�Ɠ������@�Ŏ~�܂�͂�
        if (DevelopMode && AttackFin_dev) //��U����
        {
            state = charaState.walk;
            PlayerInputSystem.cInstance.ResetStates();

            if (DevelopMode && AttackFin_dev) AttackFin_dev = false;
        }

    }

    private void DoPause(float delta = 0)
    {
        //�|�[�Y���@�W�����v�⏔�X��0�ɂ���
        //�����̕��@�ł��Ȃ�d�͂̓v���O��������ɂȂ�H
        //�@����0621:�d�͂�RighdBody�ł���Ă݂邱�ƂɁ@�\���̐ݒ��ύX����`�ł��c
        if (CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.stop);
        if (DevelopMode && Unpause_dev) //��U����
        {
            state = stockState; //�ێ��������̂ɕύX
            stockState = charaState.NONE;
            PlayerInputSystem.cInstance.Unpause_Stock();//�|�[�Y�����œ��͏�Ԃ�ێ��������̂ɕύX

            if (DevelopMode && Unpause_dev) Unpause_dev = false;
        }
    }

    #endregion
//---
}
