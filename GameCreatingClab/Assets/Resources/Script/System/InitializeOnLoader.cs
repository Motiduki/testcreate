using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�Q�l�Fhttps://kan-kikuchi.hatenablog.com/entry/ManagerSceneAutoLoader
public class InitializeOnLoader
{
    //�Q�[���J�n��(�V�[���ǂݍ��ݑO)�Ɏ��s�����@�Ȃ񂩈Ⴄ�H�H
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Application.targetFrameRate = 60;
    }
}
