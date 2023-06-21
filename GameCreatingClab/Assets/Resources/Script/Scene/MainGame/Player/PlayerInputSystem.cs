using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(PlayerMaganer))]
public class PlayerInputSystem : MonoBehaviour
{
    #region �O���Q��
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
     * ������@
    �E�J�[�\���F�Ə�����iʲ����ި��߂݂����ȃC���[�W�j
    �E�}�E�X���F�A�^�b�N
    �E�X�y�[�X�L�[or�z�C�[����F�W�����v
    �EESC�F�|�[�Y 
     */
    enum buttonState
    {
        Stundby,
        Pushing, //��������
        //Pushing_hold, //�������� ����Ȃ������H���邩���H�H
        //---������X�e�[�^�X
        Jump_Pushing, //�W�����v���̓_�b�V���̂ݎ󂯕t����
    }
    public enum buttonType
    {
        NONE = 0,
        //�Ƃ肠�����{�^������ɂ���
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
    public void ResetStates() //�X�e�[�g�̏�����
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
            case buttonState.Jump_Pushing: //�W�����v���̓A�^�b�N�����󂯕t����
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
            //�|�[�Y�ɓ���O�͏�ԃX�g�b�N
            if (_type == buttonType.Pause) stocktype = type;
            ChangeType(_type);
        }
    }
    //�|�[�Y�������Ƃ̕ێ���񔽉f
    private void ChangeType(buttonType _type)
    {
        type = _type;
        //�v���C���[��state��Ԃ��ύX����

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
