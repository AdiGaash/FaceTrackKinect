using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class KalmanFilter1D
{
    //private double Q = 0.000001;
    //private double R = 0.01;
    //private double P = 1, X = 0, K;

    private Vector3 Q;
    private Vector3 R;
    private Vector3 P, X, K;

    public KalmanFilter1D(float R, float Q)
    {
        this.R = new Vector3(R, R, R);
        this.Q = new Vector3(Q, Q, Q);
    }

    public void init(Vector3 X0, float P0)
    {
        this.P = new Vector3(P0, P0, P0);
        this.X = X0;
    }

    //private void measurementUpdate()
    //{
    //    K = new Vector3(
    //        (P.x + Q.x) / (P.x + Q.x + R.x),
    //        (P.y + Q.y) / (P.y + Q.y + R.y),
    //        (P.z + Q.z) / (P.z + Q.z + R.z)
    //        );

    //    P = new Vector3(
    //        R.x * (P.x + Q.x) / (R.x + P.x + Q.x),
    //        R.y * (P.y + Q.y) / (R.y + P.y + Q.y),
    //        R.z * (P.z + Q.z) / (R.z + P.z + Q.z)
    //        );
    //}

    public Vector3 predict()
    {
        return X;
    }

    private void predict (out Vector3 Xp, out Vector3 Pp)
    {
        Pp = new Vector3(
            R.x * (P.x + Q.x) / (R.x + P.x + Q.x),
            R.y * (P.y + Q.y) / (R.y + P.y + Q.y),
            R.z * (P.z + Q.z) / (R.z + P.z + Q.z)
            );

        Xp = X;
    }

    public Vector3 update(Vector3 measurement)
    {
        //measurementUpdate();

        //predict
        Vector3 Xp;
        Vector3 Pp;
        predict(out Xp, out Pp);



        //k
        K = new Vector3(
            (Pp.x + Q.x) / (Pp.x + Q.x + R.x),
            (Pp.y + Q.y) / (Pp.y + Q.y + R.y),
            (Pp.z + Q.z) / (Pp.z + Q.z + R.z)
            );
        
        //correct
        Vector3 tmp1 = (measurement - Xp);
        Vector3 tmp2 = new Vector3(
            tmp1.x * K.x,
            tmp1.y * K.y,
            tmp1.z * K.z
            );

        Vector3 result = Xp + tmp2;

        X = result;
        P = Pp;

        return result;
    }


}
