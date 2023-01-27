using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class OverlayTemplate : MonoBehaviour
{
    public Transform helpersGroup;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            //play mode
            helpersGroup.gameObject.SetActive(false);
        }
        else
        {
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
        }
        else
        {
            //edit mode
            lockTransform(helpersGroup.transform);
        }
    }


    private void lockTransform(Transform t)
    {
        t.localPosition = new Vector3(0, 0, 0);
        t.localRotation = Quaternion.identity;
        t.localScale = new Vector3(1, 1, 1);
    }
}
