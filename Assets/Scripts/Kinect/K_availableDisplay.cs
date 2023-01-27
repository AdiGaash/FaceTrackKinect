using UnityEngine;
using System.Collections;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using UnityEngine.UI;

public class K_availableDisplay : MonoBehaviour
{

    private KinectSensor _Sensor;
    private Text text;
    private void Awake()
    {
        text = GetComponent<Text>();
    }
    // Use this for initialization
    void Start()
    {
        _Sensor = KinectSensor.GetDefault();
    }

    // Update is called once per frame
    void Update()
    {
        string msg = "kinect | ";
        msg += _Sensor.IsAvailable ? "avail" : "not avail";
        msg += " | ";
        msg += _Sensor.IsOpen? "open" : "not open";

        text.text = msg;

    }
}
