using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Collections.Generic;
using Microsoft.Kinect.Face;

public static class K_consts
{

    public static float CAMERA_FOV = 53.8f;
    public static int FRAME_COLOR_W = 1920;
    public static int FRAME_COLOR_H = 1080;


    public static Dictionary<JointType, JointType> bonesMap = new Dictionary<JointType, JointType>()
    {
        { JointType.FootLeft, JointType.AnkleLeft },
        { JointType.AnkleLeft, JointType.KneeLeft },
        { JointType.KneeLeft, JointType.HipLeft },
        { JointType.HipLeft, JointType.SpineBase },
        { JointType.FootRight, JointType.AnkleRight },
        { JointType.AnkleRight, JointType.KneeRight },
        { JointType.KneeRight, JointType.HipRight },
        { JointType.HipRight, JointType.SpineBase },
        { JointType.HandTipLeft, JointType.HandLeft },
        { JointType.ThumbLeft, JointType.HandLeft },
        { JointType.HandLeft, JointType.WristLeft },
        { JointType.WristLeft, JointType.ElbowLeft },
        { JointType.ElbowLeft, JointType.ShoulderLeft },
        { JointType.ShoulderLeft, JointType.SpineShoulder },
        { JointType.HandTipRight, JointType.HandRight },
        { JointType.ThumbRight, JointType.HandRight },
        { JointType.HandRight, JointType.WristRight },
        { JointType.WristRight, JointType.ElbowRight },
        { JointType.ElbowRight, JointType.ShoulderRight },
        { JointType.ShoulderRight, JointType.SpineShoulder },
        { JointType.SpineBase, JointType.SpineMid },
        { JointType.SpineMid, JointType.SpineShoulder },
        { JointType.SpineShoulder, JointType.Neck },
        { JointType.Neck, JointType.Head },
    };

    public static List<FacePointType> FacePointTypesList = new List<FacePointType>()
    {
       FacePointType.EyeLeft,
       FacePointType.EyeRight,
       FacePointType.Nose,
       FacePointType.MouthCornerLeft,
       FacePointType.MouthCornerRight,       
    };

}

