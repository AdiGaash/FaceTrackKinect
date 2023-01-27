using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using yak;

public class TestSSensor : MonoBehaviour
{

    [Ctrl]
    public Color ccc;


    [Ctrl("factor 1 is cool")]
    public float factor1 { get; set; }

    [Ctrl("but i am also ")]
    public int factor2 { get; set; }

    [CtrlSlider(min = 0, max = 10)]
    public float factor3;

    [CtrlSlider(min = 0, max = 100)]
    public int factor4;


    private void Update()
    {

        if (UnityEngine.Random.Range(0, 100) >= 99)
        {
            factor1 = UnityEngine.Random.Range(0, 1f);
        }

        if (UnityEngine.Random.Range(0, 100) >= 99)
        {
            factor2 = UnityEngine.Random.Range(0, 199);
        }

        if (UnityEngine.Random.Range(0, 100) >= 99)
        {
            factor3 = UnityEngine.Random.Range(0, 10);
        }

        if (UnityEngine.Random.Range(0, 100) >= 89)
        {
            ccc = UnityEngine.Random.ColorHSV();
        }

    }
}

