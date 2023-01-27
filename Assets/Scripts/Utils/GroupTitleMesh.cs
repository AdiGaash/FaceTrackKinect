using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(TextMesh))]
public class GroupTitleMesh : MonoBehaviour
{
    TextMesh text;

    private void Awake()
    {
        text = GetComponent<TextMesh>();
    }
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            var parent = transform.parent.gameObject;

            var count = 0;
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i);
                if (child.GetComponent<OverlayTemplate>() != null)
                    count++;
            }
            text.text = parent.name + "\n" + (count) + " items";



        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
