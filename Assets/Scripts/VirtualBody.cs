using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Kinect.Face;
using UnityEngine;
using Windows.Kinect;
using Kinect = Windows.Kinect;

public class VirtualBody : MonoBehaviour
{
    public Sprite faceJointSprite;
    public Material boneMaterial;
    public Material jointMaterial;

    public UnityEngine.Color facePointColor = new UnityEngine.Color(0, 0, 1, 0.5f);

    [Header("** hierarchy **")]
    public GameObject bones;
    public GameObject faceBones;
    public GameObject faceStencil;
    public GameObject faceTracker3D;
    public GameObject overlay3D;

    [Space]
    [Space]
    [Header("** don't touch **")]
    public BodiesVirtualizer virtualizer;
    public ulong trackingId;

    private bool firstUpdate = true;
    private bool started = false;
    private float lastFaceDetectTime;
    public const float FACE_DETECT_TIMEOUT = 1.5f; // hide faces without detection for X time.

    //filter
    private KalmanFilter1D filter;
    private float Q = 0.0001f;
    private float R = 0.01f;
    private float P0 = 1;

    // -------------------------------------------------------------------
    //statistics
    private bool logStatistics = false;
    private Quaternion lastOr;
    private Vector3 lastOrEuler;

    StatisticsCollector<Vector3> statisticsPosition;
    StatisticsCollector<Quaternion> statisticsOrientation;

    StatisticsCollector<Vector3> statisticsPositionF;
    StatisticsCollector<Quaternion> statisticsOrientationF;



    // -------------------------------------------------------------------
    // -------------------------------------------------------------------
    private void Awake()
    {
        virtualizer = FindObjectOfType<BodiesVirtualizer>();
        filter = new KalmanFilter1D(R, Q);

        statisticsPosition = new StatisticsCollector<Vector3>();
        statisticsOrientation = new StatisticsCollector<Quaternion>();

        statisticsPositionF = new StatisticsCollector<Vector3>();
        statisticsOrientationF = new StatisticsCollector<Quaternion>();
    }

    void Start()
    {
        name = "Body:" + trackingId;

        for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++)
        {
            //joint
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            jointObj.transform.SetParent(bones.transform, false);
            jointObj.name = jt.ToString();
            MeshRenderer mr = jointObj.GetComponent<MeshRenderer>();
            mr.material = jointMaterial;

            jointObj.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
            if (jt == JointType.Head)
            {
                jointObj.transform.localScale = new Vector3(0.1f, 0.15f, 0.03f);
            }

            //bone
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = boneMaterial;
            lr.startWidth = 0.01f;
            lr.endWidth = 0.01f;
            lr.useWorldSpace = true;
        }

        //build facePoints
        foreach (var pointType in K_consts.FacePointTypesList)
        {
            //GameObject facePoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject facePoint = new GameObject();
            var pointSprite = facePoint.AddComponent<SpriteRenderer>();
            pointSprite.sprite = this.faceJointSprite;
            pointSprite.color = facePointColor;

            facePoint.transform.SetParent(faceBones.transform, false);
            facePoint.name = pointType.ToString();
            facePoint.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            //MeshRenderer mr = facePoint.GetComponent<MeshRenderer>();
            //mr.material = FaceJointMaterial;
        }

        started = true;
    }

    void Update()
    {
        updateVisibilities();

    }

    private void OnDestroy()
    {
        if (statisticsOrientation.history.Count > 0)
        {
            Debug.Log(statisticsOrientation.dump(x => string.Format("{0}\t{1}\t{2}\t{3}", x.x, x.y, x.z, x.w)));
            Debug.Log(statisticsOrientationF.dump(x => string.Format("{0}\t{1}\t{2}\t{3}", x.x, x.y, x.z, x.w)));
        }
    }
    // -------------------------------------------------------------------
    // -------------------------------------------------------------------
    public void updateVisibilities()
    {
        bones.SetActive(virtualizer.showBones);
        faceBones.SetActive(virtualizer.showFaceJoints);
        faceStencil.SetActive(virtualizer.showStencil);
        faceTracker3D.SetActive(virtualizer.showFaceTrackers3D);
        overlay3D.SetActive(virtualizer.showOverlays3D);

        //hide ovelays for faces without detects for a long time
        if (Time.time > lastFaceDetectTime + FACE_DETECT_TIMEOUT)
        {
            faceStencil.SetActive(false);
            overlay3D.SetActive(false);
        }

        //if (
        //    (transform.localPosition.z > virtualizer.factors.limitZMax) ||
        //    (transform.localPosition.z < virtualizer.factors.limitZMin)
        //    )
        //{
        //    faceStencil.SetActive(false);
        //    overlay3D.SetActive(false);
        //}

    }

    public GameObject set3DOverlay(GameObject overlay)
    {
        //remove old:
        foreach (Transform child in overlay3D.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //create new
        overlay.transform.SetParent(overlay3D.transform, false);
        overlay.transform.localPosition = new Vector3(0, 0, 0);
        overlay.transform.localRotation = Quaternion.identity;
        overlay.transform.localScale = new Vector3(1, 1, 1);
        return overlay;
    }

    // -------------------------------------------------------------------


    public void RefreshBodyObject(CoordinateMapper coordMapper, Body body, FaceFrameResult face)
    {
        if (!started)
        {
            return;
        }

        var now = Utils.currentTimeMills;

        //face orientation (quaternion and euler)
        Vector3 facePosition;
        Quaternion faceOrientation;
        Vector3 faceOrientationEuler;

        bool hasFaceDetect = GetFaceCoords(coordMapper, virtualizer.virtualCamera, body, face, out faceOrientation, out facePosition);
        faceOrientationEuler = K_utils.getEuler(faceOrientation);
        faceOrientationEuler = faceOrientationEuler * Mathf.Deg2Rad;

        if (hasFaceDetect)
        {
            lastFaceDetectTime = Time.time;
            lastOr = faceOrientation;
            lastOrEuler = faceOrientationEuler;
        }

        if (logStatistics)
        {
            statisticsPosition.add(now, facePosition);
            statisticsOrientation.add(now, lastOr);
        }

        //smoothing raw face positions data
        if (virtualizer.smoothFilter)
        {
            //if (hasFaceDetect)
            if (firstUpdate)
            {
                //init
                filter.init(lastOrEuler, P0);
                firstUpdate = false;
            }
            else
            {
                //correct
                faceOrientationEuler = filter.update(lastOrEuler);
                faceOrientation = Quaternion.Euler(faceOrientationEuler * Mathf.Rad2Deg);
            }
        }

        //collect raw data statitics
        if (logStatistics)
        {
            statisticsPositionF.add(now, facePosition);
            statisticsOrientationF.add(now, faceOrientation);
        }

        //set face position
        faceStencil.transform.localPosition = facePosition;
        faceTracker3D.transform.localPosition = facePosition;
        overlay3D.transform.localPosition = facePosition;

        //set face rotation
        faceStencil.transform.rotation = faceOrientation;
        faceTracker3D.transform.rotation = faceOrientation;
        overlay3D.transform.rotation = faceOrientation;

        //joints
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {

            Transform jointObj = bones.transform.Find(jt.ToString());

            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;

            if (K_consts.bonesMap.ContainsKey(jt))
                targetJoint = body.Joints[K_consts.bonesMap[jt]];

            //hide untracked joints
            if (sourceJoint.TrackingState == TrackingState.Tracked)
            {
                jointObj.gameObject.SetActive(true);
            }
            else
            {
                jointObj.gameObject.SetActive(false);
                continue;
            }

            jointObj.localPosition = GetVector3FromJoint(coordMapper, virtualizer.virtualCamera, sourceJoint);

            //orientation
            var orientationKinect = body.JointOrientations[jt];
            var orientation = K_utils.convertKinectQuarternion(orientationKinect.Orientation);
            jointObj.rotation = orientation;

            if (jt == Kinect.JointType.Head)
                jointObj.rotation = faceOrientation;

            //lines
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if (targetJoint.HasValue && targetJoint.Value.TrackingState == TrackingState.Tracked)
            {
                lr.SetPosition(0, jointObj.position);
                lr.SetPosition(1, transform.position + GetVector3FromJoint(coordMapper, virtualizer.virtualCamera, targetJoint.Value));
                lr.startColor = GetColorForState(sourceJoint.TrackingState);
                lr.endColor = GetColorForState(targetJoint.Value.TrackingState);
            }
            else
            {
                lr.enabled = false;
            }
        }

        //Face joints (face joints are in 2D color space, project them using the camera!)
        if (face != null)
        {
            foreach (var pointType in K_consts.FacePointTypesList)
            {
                Point sourceFaceJoint;
                if (face.FacePointsInColorSpace.TryGetValue(pointType, out sourceFaceJoint))
                {
                    Transform jointObj = faceBones.transform.Find(pointType.ToString());
                    var spriteRenderer = jointObj.GetComponent<SpriteRenderer>();

                    //detected a point (not (0,0) )??
                    if (sourceFaceJoint.X != 0 && sourceFaceJoint.Y != 0)
                    {
                        //project 2d point to world
                        Vector3 posColor = new Vector3(
                                        sourceFaceJoint.X / K_consts.FRAME_COLOR_W * virtualizer.virtualCamera.pixelWidth,
                                        (K_consts.FRAME_COLOR_H - sourceFaceJoint.Y) / K_consts.FRAME_COLOR_H * virtualizer.virtualCamera.pixelHeight,
                                        virtualizer.virtualCamera.nearClipPlane
                                    );
                        jointObj.position = virtualizer.virtualCamera.ScreenToWorldPoint(posColor);

                        spriteRenderer.color = facePointColor;
                    }
                    else
                    {
                        //stay in the same position...
                        spriteRenderer.color = new UnityEngine.Color(facePointColor.r, facePointColor.g, facePointColor.b, 0.3f);
                    }
                }
            }
        }

    }

    private bool GetFaceCoords(CoordinateMapper coordMapper, Camera camera, Body body, FaceFrameResult face, out Quaternion orientation, out Vector3 position)
    {
        orientation = new Quaternion();
        position = new Vector3();

        //bails
        if (body == null)
            return false;

        //default body joint
        position = GetVector3FromJoint(coordMapper, camera, body.Joints[JointType.Head]);
        orientation = K_utils.convertKinectQuarternion(body.JointOrientations[JointType.Head].Orientation);

        if (face == null)
            return false;

        //did kinect find the face points?
        bool hasFaceDetect = false;
        foreach (var pointType in K_consts.FacePointTypesList)
        {
            Point sourceFaceJoint;
            if (face.FacePointsInColorSpace.TryGetValue(pointType, out sourceFaceJoint))
            {
                //detected a point (not (0,0) )??
                if (
                    Math.Abs(sourceFaceJoint.X) > 0.000001 &&
                    Math.Abs(sourceFaceJoint.Y) > 0.000001
                    )
                {
                    hasFaceDetect = true;
                }
            }
        }

        //output
        orientation = K_utils.convertKinectQuarternion(face.FaceRotationQuaternion);
        return hasFaceDetect;
    }

    private static UnityEngine.Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
            case Kinect.TrackingState.Tracked:
                return UnityEngine.Color.green;

            case Kinect.TrackingState.Inferred:
                return UnityEngine.Color.red;

            default:
                return UnityEngine.Color.black;
        }
    }

    private Vector3 GetVector3FromJoint(CoordinateMapper coordMapper, Camera camera, Kinect.Joint joint)
    {
        if (joint.TrackingState != TrackingState.Tracked)
            return new Vector3();

        //1. bad option : direct position from joint.
        //   this is wrong because of the offset between color and depth camera
        //   plus the distortion of the color camera
        //return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);


        //2. backProject:
        //   not ideal but works well.
        //   we'll ask the coordinate mapper to map the position (depth camera space) on the color image (color camera space)
        //   (and resize it to the unity camera image size, and filp Y axis to be in the unity coords space)
        //   then we'll project a ray from the unity-camera to the screen point ,
        //   with a length of the original position (this isn't perfect be a good aproximation)

        float distance = Mathf.Sqrt(joint.Position.X * joint.Position.X + joint.Position.Y * joint.Position.Y + joint.Position.Z * joint.Position.Z);
        var colorSpacePnt = coordMapper.MapCameraPointToColorSpace(joint.Position);
        var screenPnt = new Vector3(
                                colorSpacePnt.X
                                / K_consts.FRAME_COLOR_W * camera.pixelWidth,

                                (K_consts.FRAME_COLOR_H - colorSpacePnt.Y)
                                / K_consts.FRAME_COLOR_H * camera.pixelHeight,

                                1f /*ignored*/);

        var ray = camera.ScreenPointToRay(screenPnt);
        return ray.direction.normalized * distance;
    }
}
