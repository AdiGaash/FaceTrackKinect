using UnityEngine;
using System.Collections;
using Windows.Kinect;
using Microsoft.Kinect.Face;

public class K_BodySrcMgr : MonoBehaviour
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;

    private Body[] _Data = null;

    private int bodyCount;
    private FaceFrameResult[] _FaceData = null;
    private FaceFrameSource[] faceFrameSources;
    private FaceFrameReader[] faceFrameReaders;
    private CoordinateMapper _coordsMapper;

    public Body[] GetBodyDate()
    {
        return _Data;
    }

    public CoordinateMapper getCoordsMapper()
    {
        return _coordsMapper;
    }

    public FaceFrameResult[] GetFaceData()
    {
        return _FaceData;
    }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();

            // Initialize the face source with the desired features
            bodyCount = _Sensor.BodyFrameSource.BodyCount;

            // specify the required face frame results
            FaceFrameFeatures faceFrameFeatures =
                FaceFrameFeatures.BoundingBoxInColorSpace
                    | FaceFrameFeatures.PointsInColorSpace
                    | FaceFrameFeatures.BoundingBoxInInfraredSpace
                    | FaceFrameFeatures.PointsInInfraredSpace
                    | FaceFrameFeatures.RotationOrientation
                    | FaceFrameFeatures.FaceEngagement
                    | FaceFrameFeatures.Glasses
                    | FaceFrameFeatures.Happy
                    | FaceFrameFeatures.LeftEyeClosed
                    | FaceFrameFeatures.RightEyeClosed
                    | FaceFrameFeatures.LookingAway
                    | FaceFrameFeatures.MouthMoved
                    | FaceFrameFeatures.MouthOpen;

            // create a face frame source + reader to track each face in the FOV
            faceFrameSources = new FaceFrameSource[bodyCount];
            faceFrameReaders = new FaceFrameReader[bodyCount];
            _FaceData = new FaceFrameResult[bodyCount];

            for (int i = 0; i < bodyCount; i++)
            {
                // create the face frame source with the required face frame features and an initial tracking Id of 0
                faceFrameSources[i] = FaceFrameSource.Create(_Sensor, 0, faceFrameFeatures);

                // open the corresponding reader
                faceFrameReaders[i] = faceFrameSources[i].OpenReader();

                //face data
                //_FaceData[i] = new Face();
            }

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
            _coordsMapper = _Sensor.CoordinateMapper;
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(_Data);
                frame.Dispose();
                frame = null;
            }
        }

        //FACE
        // iterate through each body and update face source
        for (int i = 0; i < bodyCount; i++)
        {
            // check if a valid face is tracked in this face source				
            if (faceFrameSources[i].IsTrackingIdValid)
            {
                using (FaceFrame frame = faceFrameReaders[i].AcquireLatestFrame())
                {
                    if (frame != null)
                    {
                        if (frame.TrackingId == 0)
                        {
                            continue;
                        }

                        _FaceData[i] = frame.FaceFrameResult;
                    }
                }
            }
            else
            {
                // check if the corresponding body is tracked 
                if (_Data != null && _Data[i] != null && _Data[i].IsTracked)
                {
                    // update the face frame source to track this body
                    faceFrameSources[i].TrackingId = _Data[i].TrackingId;
                }
                else
                {
                    faceFrameSources[i].TrackingId = 0;
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}