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
        idol, //�Î~���@�ǂɓ������Ă钆��
        walk,
        jump,
        attack,
        jump_attack, //jump & attack�Ŕ���
        //�_�b�V����jump+attack�ŉ\
        pause,
    }
    [SerializeField, ReadOnly,Header("�Q�[�����̌��݂̃L�����X�e�[�^�X")]
    charaState state = charaState.Init;
    [SerializeField,ReadOnly]charaState stockState = charaState.NONE; //�|�[�Y����߂�ہA�ێ�����X�e�[�^�X
    [SerializeField, ReadOnly] PlayerStatu nowStatus;

    private Rigidbody rb;
    //�W�����v�p
    private bool falling; //�������t���O
    [SerializeField, ReadOnly] float y = 0, y_vel;
    [SerializeField, Header("�ő�̃W�����v�p���[")] float y_vel_max = 35;  //�����x
    [SerializeField, Header("�W�����v�iup�j�̉��Z�l")] float y_a1 = 4;
    [SerializeField, Header("�W�����v�idown�j�̉��Z�l")] float y_a2 = 2; //�W�����v�A�b�v���̎��́A�_�E�����̉����x
    //�A�^�b�N
    [SerializeField] private GameObject bulletObj;
    private bool ButtelCreate_Once = false; //�ʔ��˂������ǂ���
    private float stayTime = 0;//�A�j�����Ȃ����͂���Ńe�L�g�[�ɂ�����
    //�W�����v�A�^�b�N
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
        //�J�����ƃL�����𓮂���
        RandChecker.cInstance.changeRand(true);
        rb.isKinematic = false;

        //�J�����������ĂȂ���Γ�����
        if (!CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.moving);

        //���͎�t�����ł��
        switch (PlayerInputSystem.cInstance.IsType) 
        {
            case buttonType.Jump:
                //�W�����v����O�̐ݒ���
                y_vel = y_vel_max;
                y = y_vel;
                falling = false;
                RandChecker.cInstance.changeRand(false);

                state = charaState.jump;
                break;
            case buttonType.Attack:
                //�A�^�b�N�O�̐ݒ���
                stayTime = 0; //�҂����Ԑݒ�
                ButtelCreate_Once = false;

                state = charaState.attack;
                break;

            default:
                break;
        }

    }
    private void DoJump(float delta)
    {
        //�����X�ђʂ���̂ł���C���\��

        //�J�����ƃL�����𓮂���
        //�J�����������ĂȂ���Γ�����
        if (!CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.moving);
        rb.isKinematic = false;

        //�A�^�b�N�̓��͂���������attack�ɐ؂�ւ�
        if (PlayerInputSystem.cInstance.IsType == buttonType.Attack)
        {
            stayTime = 0;
            ButtelCreate_Once = false;

            //�W�����v�̒��_�����ݒn�Ƃ���
            y_vel = 0;

            falling = true;
            state = charaState.jump_attack;
        }

        //�㏸
        //if (!RandChecker.cInstance.IsRanding) //�n�ʂɂ��ĂȂ��Ƃ�
        if(!falling)
        {
            //�r���Ń{�^���𗣂����A�܂��͒��_�ɗ�����
            if (y_vel <= 0)
            {
                y_vel = 0;
                falling = true;
            }
            else
            {
                //�W�����v
                y_vel += -y_a1;
                y = y_vel;

            }
        }
        //����
        else
        {
            //���n��������͏�ԃ��Z�b�g�iInputManager->Init�j
            //if (DevelopMode && RandMode_dev) //��U����
            if (RandChecker.cInstance.IsRanding)
            {
                //���n�p�ɍĐݒ�
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
        //�L�����̑O������΂ߏ㔭�˒e���꒼����ɏo��
        //�J�[�\�������̏���
        //�J�����������ĂȂ���Γ�����

        //���˃��[�V�������͒�~
        //CameraManager.cInstance.ChangeState(CameraState.stop);
        //���[�V�����I����walk�ֈȍ~�A���͏�Ԃ����Z�b�g

        if (CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.stop);

        if (!ButtelCreate_Once)
        {
            Transform Intantiate_Parent = this.transform.parent;
            GameObject bullet = Instantiate(bulletObj, Intantiate_Parent); //�e�̐���

            ButtelCreate_Once = true;
            
        }
        else
        {
            if (stayTime <= 1.0f)
            {
                stayTime += delta;
            }
            //�K���ɑ҂�����
            //if (DevelopMode && AttackFin_dev) //��U����
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
        //�o�b�N�W�����v�i�W�����v�~�A�^�b�N�j
        //�W�����v���A�^�b�N�̏������󂯕t���Ȃ�

        /*
        �@�A�j���[�V�����̃C���[�W�@�������b�������Ō��߂�
        �@�P�D�W�����v����
        �@�Q�D�A�^�b�N�J�n�ŁAposY�͂��̂܂܂�posX���ړ��@���̎����͏�Ԃ�attack�ɂȂ�
          �R�D�A�^�b�N���[�V�����I���ŗ����J�n�i�A�^�b�N�����������_�Ń\�R�����_�����ɂȂ�j
        �@�S�D���n��state=walk & ���͏�ԃ��Z�b�g
         */



        if (!ButtelCreate_Once)
        {
            if (CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.stop);
            Transform Intantiate_Parent = this.transform.parent;
            GameObject bullet = Instantiate(bulletObj, Intantiate_Parent); //�e�̐���
            CreateBullet = bullet.GetComponent<BulletController>();

            ButtelCreate_Once = true;

        }
        else
        {
            if (CreateBullet.IsInit) return; //�c��񏉊��ݒ蒆�͒ʂ�Ȃ�

            //�҂�����
            if (stayTime <= 1.0f)
            {
                stayTime += delta;
            }
            
            //�d�͗�������
            //���n��������͏�ԃ��Z�b�g�iInputManager->Init�j
            if (RandChecker.cInstance.IsRanding)
            {
                CameraManager.cInstance.ChangeState(CameraState.stop);
                y = 0;
            }
            else
            {
                //������
                if (CreateBullet.getMousePos.x >= Screen.width / 2) 
                    CameraManager.cInstance.ChangeState(CameraState.back_moving);
                else
                    CameraManager.cInstance.ChangeState(CameraState.dash_moving);
                y_vel += y_a2;
                y = -y_vel;

            }
            rb.velocity = new Vector2(0, y);

            //�����Ƒ҂����Ԃ��I�������I��
            if (stayTime > 1.0f && RandChecker.cInstance.IsRanding)
            {
                stayTime = 0;
                state = charaState.walk;
                PlayerInputSystem.cInstance.ResetStates();

                //���n�p�ɍĐݒ�
                y = 0;
                falling = false;

                state = charaState.walk;
                PlayerInputSystem.cInstance.ResetStates();
            }   
        }

        /*
        //�ŏI�I��type=attack�ɂȂ�Ȃ�Aattack�Ɠ������@�Ŏ~�܂�͂�
        if (DevelopMode && AttackFin_dev) //��U����
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
        //�|�[�Y���@�W�����v�⏔�X��0�ɂ���
        //�����̕��@�ł��Ȃ�d�͂̓v���O��������ɂȂ�H
        //�@����0621:�d�͂�RighdBody�ł���Ă݂邱�ƂɁ@�\���̐ݒ��ύX����`�ł��c
        if (CameraManager.cInstance.IsMovingCamera) CameraManager.cInstance.ChangeState(CameraState.stop);

        if (PlayerInputSystem.cInstance.IsType == buttonType.Unpause)
        //if (DevelopMode && Unpause_dev) //��U����
        {
            state = stockState; //�ێ��������̂ɕύX
            stockState = charaState.NONE;
            PlayerInputSystem.cInstance.Unpause_Stock();//�|�[�Y�����œ��͏�Ԃ�ێ��������̂ɕύX
        }
    }

    #endregion
}
