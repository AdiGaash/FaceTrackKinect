using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Windows.Kinect;

public static class K_utils
{
    public static Quaternion convertKinectQuarternion(Windows.Kinect.Vector4 vec)
    {
        var res = new Quaternion(vec.X, vec.Y, vec.Z, vec.W);
        return res;
    }

    public static Vector3 getEuler(Windows.Kinect.Vector4 vec, bool normalizeRange = true)
    {
        var q = convertKinectQuarternion(vec);
        return getEuler(q, normalizeRange);
    }

    public static Vector3 getEuler(Quaternion q, bool normalizeRange = true)
    {
        var res = q.eulerAngles;
        if (normalizeRange)
            normalizeEulerRange(ref res);

        return res;
    }

    public static Vector3 normalizeEulerRange(ref Vector3 v3)
    {
        while (v3.x < -180) v3.x += 360;
        while (v3.y < -180) v3.y += 360;
        while (v3.z < -180) v3.z += 360;

        while (v3.x > 180) v3.x -= 360;
        while (v3.y > 180) v3.y -= 360;
        while (v3.z > 180) v3.z -= 360;

        return v3;
    }

    // -------------------------------------------------------------------
    // -------------------------------------------------------------------

    public struct CameraIntrinsics
    {
        public float cx, cy;
        public float fx, fy;
        public float k1, k2, k3;
        public float p1, p2;
    }

    public static PointF[] distortPoints(CameraIntrinsics cam, PointF[] points)
    {
        var res = new PointF[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            res[i] = distortPoint(cam, points[i]);
        }
        return res;
    }

    public static PointF distortPoint(CameraIntrinsics cam, PointF point)
    {
        // To relative coordinates 
        double x = (point.X - cam.cx) / cam.fx;
        double y = (point.Y - cam.cy) / cam.fy;

        double r2 = x * x + y * y;

        // Radial distorsion
        double xDistort = x * (1 + cam.k1 * r2 + cam.k2 * r2 * r2 + cam.k3 * r2 * r2 * r2);
        double yDistort = y * (1 + cam.k1 * r2 + cam.k2 * r2 * r2 + cam.k3 * r2 * r2 * r2);

        // Tangential distorsion
        xDistort = xDistort + (2 * cam.p1 * x * y + cam.p2 * (r2 + 2 * x * x));
        yDistort = yDistort + (cam.p1 * (r2 + 2 * y * y) + 2 * cam.p2 * x * y);

        // Back to absolute coordinates.
        xDistort = xDistort * cam.fx + cam.cx;
        yDistort = yDistort * cam.fy + cam.cy;

        return new PointF() { X = (float)xDistort, Y = (float)yDistort };
    }
}