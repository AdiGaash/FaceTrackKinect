using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class test1 : MonoBehaviour
{
    public Vector3 e;
    private KinectSensor _Sensor;

    private void Awake()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
        
        Debug.LogFormat(
            "fov XY {0}\n" +
            "fov X  {1}\n" +
            "fov Y  {2}\n",
            _Sensor.ColorFrameSource.FrameDescription.DiagonalFieldOfView,
            _Sensor.ColorFrameSource.FrameDescription.HorizontalFieldOfView,
            _Sensor.ColorFrameSource.FrameDescription.VerticalFieldOfView);

        /*
         kinect color camera and depth relative Rotation matrix
        [ 9.9984628826577793e-01, 1.2635359098409581e-03,-1.7487233004436643e-02, 
        -1.4779096108364480e-03, 9.9992385683542895e-01, -1.2251380107679535e-02,
        1.7470421412464927e-02, 1.2275341476520762e-02, 9.9977202419716948e-01 ]

                    9.9984628826577793e-01f; 
                    1.2635359098409581e-03f;
                    -1.7487233004436643e-02f; 

                    -1.4779096108364480e-03f;
                    9.9992385683542895e-01f;
                    -1.2251380107679535e-02f;

                    1.7470421412464927e-02f;
                    1.2275341476520762e-02f;
                    9.9977202419716948e-01f;        
         */

        var m = new Matrix4x4();
        m[0, 0] = 9.9984628826577793e-01f;
        m[0, 1] = 1.2635359098409581e-03f;
        m[0, 2] = -1.7487233004436643e-02f;
        m[0, 3] = 0;

        m[1, 0] = -1.4779096108364480e-03f;
        m[1, 1] = 9.9992385683542895e-01f;
        m[1, 2] = -1.2251380107679535e-02f;
        m[1, 3] = 0;

        m[2, 0] = 1.7470421412464927e-02f;
        m[2, 1] = 1.2275341476520762e-02f;
        m[2, 2] = 9.9977202419716948e-01f;
        m[2, 3] = 0;

        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = 0;
        m[3, 3] = 1;

        //convert to eualer degrees:
        Quaternion q = QuaternionFromMatrix(m);
        e = q.eulerAngles;

        while (e.x < -180) e.x += 360;
        while (e.y < -180) e.y += 360;
        while (e.z < -180) e.z += 360;

        while (e.x > 180) e.x -= 360;
        while (e.y > 180) e.y -= 360;
        while (e.z > 180) e.z -= 360;
    }


    void OnApplicationQuit()
    {
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }

    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }


}
