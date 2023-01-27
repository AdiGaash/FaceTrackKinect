using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DisplayConfigJSON
{
    public bool hideCursor = true;

    public bool forceFantastic = true;

    public bool forceFullScreen = true;

    public bool forceResolution = true;
    public int forceRsolutionWidth = 1920;
    public int forceRsolutionHeight = 1080;
}
