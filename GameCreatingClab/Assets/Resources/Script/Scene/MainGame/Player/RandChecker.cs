using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandChecker : MonoBehaviour
{
    #region@ŠO•”ŽQÆ
    public static RandChecker cInstance = null;

    private void Awake()
    {
        if (cInstance == null)
        {
            cInstance = this;
        }
    }
    #endregion

    [SerializeField,ReadOnly]private bool randing;
    public bool IsRanding => randing;
    public void changeRand(bool _f = true) => randing = _f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "floor")
        {
            randing = true;
        }
    }

    /*
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "floor")
        {
            randing = false;
        }
    }
    */

}
