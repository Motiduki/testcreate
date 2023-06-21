using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    [SerializeField] GameObject PauseObj_Par;
    private void Start()
    {
        PauseObj_Par.SetActive(false);
    }
    private void Update()
    {
        if (PlayerInputSystem.cInstance == null) return;

        PauseObj_Par.SetActive(PlayerInputSystem.cInstance.IsPausing());
    }
}
