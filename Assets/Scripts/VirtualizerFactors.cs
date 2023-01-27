using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using yak;

[Serializable]
public class VirtualizerFactors
{
    [CtrlSlider(label = "zoom", min = 1, max = 10, numberFormat = "0.00")]
    public float zoom = 1;

    [CtrlSlider(label = "offset horizontal", min = -1, max = 1, numberFormat = "0.00")]
    public float offsetX = 0f;

    [CtrlSlider(label = "offset vertical", min = -1, max = 1, numberFormat = "0.00")]
    public float offsetY = 0f;

    //[CtrlSlider(label = "limit Z min", min = 0, max = 10, numberFormat = "0.00")]
    //public float limitZMin = 0;

    //[CtrlSlider(label = "limit Z max", min = 0, max = 10, numberFormat = "0.00")]
    //public float limitZMax = 0;


}

