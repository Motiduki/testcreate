using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    [SerializeField,Header("�f�o�b�O�I�u�W�F�̗L����")] bool DebugMode;
    [SerializeField] GameObject MainObj;

    [SerializeField] Text fps_tx;
    private void Update()
    {
        MainObj.SetActive(DebugMode);

        if (DebugMode) 
        {
            //fps�\��
            float fps = 1f / Time.deltaTime;//�v�Z
            fps_tx.text = "fps:" + fps.ToString();
        }
    }
    
}
