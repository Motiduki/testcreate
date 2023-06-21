using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    [SerializeField,Header("デバッグオブジェの有効化")] bool DebugMode;
    [SerializeField] GameObject MainObj;

    [SerializeField] Text fps_tx;
    private void Update()
    {
        MainObj.SetActive(DebugMode);

        if (DebugMode) 
        {
            //fps表示
            float fps = 1f / Time.deltaTime;//計算
            fps_tx.text = "fps:" + fps.ToString();
        }
    }
    
}
