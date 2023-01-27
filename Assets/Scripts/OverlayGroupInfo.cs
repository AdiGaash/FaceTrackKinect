using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OverlayGroupInfo
{
    public bool enabled = true;
    public int timeSeconds = 120;
    public string name;

    public OverlayGroupInfo()
    {
    }

    public OverlayGroupInfo(string name)
    {
        this.name = name;
    }
}