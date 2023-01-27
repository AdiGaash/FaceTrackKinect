using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public class OverlayGroupsJson
{
    public int defaultTimeSeconds = 120;
    public List<OverlayGroupInfo> groups;

    public OverlayGroupsJson()
    {
    }    
}
