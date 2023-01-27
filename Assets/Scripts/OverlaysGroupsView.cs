using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlaysGroupsView : MonoBehaviour
{
    public BodiesVirtualizer virtualizer;

    public RectTransform groupsListView;

    public Color activeTextColor;
    public Color nonActiveTextColor;
    public Color disabledTextColor;

    private bool isInit = false;
    private List<OverlayGroup> allGroups;
    private OverlayGroup currentGroup;

    private void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //first time
        if (!isInit)
            initGroups();

        if (isInit)
        {
            var newGroup = virtualizer.currentOverlaysGroup;
            if (newGroup != currentGroup)
            {
                for (int i = 0; i < groupsListView.childCount; i++)
                {
                    var child = groupsListView.GetChild(i);

                    var grp = allGroups.FirstOrDefault(x => x.info.name == child.gameObject.name);
                    if (grp == null)
                        continue;

                    var txt = child.GetComponentInChildren<Text>();
                    txt.fontStyle = FontStyle.Normal;
                    txt.color = nonActiveTextColor;

                    //gray out disabled
                    if (!grp.info.enabled)
                        txt.color = disabledTextColor;
                }

                currentGroup = newGroup;
                var currentBtn = groupsListView.Find(currentGroup.info.name);
                var currentTxt = currentBtn.GetComponentInChildren<Text>();
                currentTxt.color = activeTextColor;
                currentTxt.fontStyle = FontStyle.Bold;

            }
        }
    }

    private void initGroups()
    {
        allGroups = virtualizer.overlayGroups;
        if (allGroups == null)
            return;

        //get template button:
        var btnTemplate = groupsListView.GetChild(0);

        foreach (var grp in allGroups)
        {
            var grpBtn = (GameObject)Instantiate(btnTemplate.gameObject);
            grpBtn.name = grp.info.name;
            var txt = grpBtn.GetComponentInChildren<Text>();
            txt.text = string.Format("{0} [{1} hats]\n{2} seconds", grp.info.name, grp.overlays.Count, grp.info.timeSeconds);

            grpBtn.transform.SetParent(groupsListView, false);
        }
        btnTemplate.gameObject.SetActive(false);
        isInit = true;
    }
}
