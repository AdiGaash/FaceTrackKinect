using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class K_ColorView : MonoBehaviour
{
    public K_ColorSrcMgr ColorSourceManager;
    private RawImage compRawImage;

    void Awake()
    {
        compRawImage = gameObject.GetComponent<RawImage>();
        compRawImage.uvRect = new Rect(0, 0, 1, -1);
    }
    
    void Update()
    {
        compRawImage.texture = ColorSourceManager.GetColorTexture();
    }
}