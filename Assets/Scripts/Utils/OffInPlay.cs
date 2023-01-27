using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffInPlay : MonoBehaviour
{

    public bool offInPlayMode = true;

    private void Awake()
    {
        if (offInPlayMode)
        {
            gameObject.SetActive(false);
        }
    }
}
