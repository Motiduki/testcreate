using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//参考：https://kan-kikuchi.hatenablog.com/entry/ManagerSceneAutoLoader
public class InitializeOnLoader
{
    //ゲーム開始時(シーン読み込み前)に実行される　なんか違う？？
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Application.targetFrameRate = 60;
    }
}
