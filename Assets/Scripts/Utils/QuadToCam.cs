using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class QuadToCam : MonoBehaviour
{
    public Camera cam;
    public float distanceFactor = .95f;
    public Vector3 rotationFactor;
    public Vector3 scaleFactor = new Vector3(1, 1, 1);

    
    private void Awake()
    {

    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (cam != null)
        {
            transform.localScale = new Vector3(1, 1, 1);

            var f = cam.farClipPlane * distanceFactor;
            var h = 2 * f * Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView / 2);
            var w = h * cam.aspect;
            transform.position = cam.transform.position + cam.transform.forward * f;
            transform.localScale = new Vector3(
                                                scaleFactor.x * (transform.localScale.x / transform.lossyScale.x * w),
                                                scaleFactor.y * (transform.localScale.y / transform.lossyScale.y * h),
                                                scaleFactor.z * (transform.localScale.z / transform.lossyScale.z)
                                              );
            transform.rotation = cam.transform.rotation * Quaternion.Euler(rotationFactor);

        }
    }
}
