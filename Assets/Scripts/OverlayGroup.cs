using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayGroup
{
    public OverlayGroupInfo info;
    public List<GameObject> overlays;

    public OverlayGroup(string name, List<GameObject> overlays)
    {
        this.info = new OverlayGroupInfo(name);
        this.overlays = overlays;
    }
}
