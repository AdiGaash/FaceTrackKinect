using Microsoft.Kinect.Face;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using System.IO;
using yak;

public class BodiesVirtualizer : MonoBehaviour
{
    //sources
    public K_BodySrcMgr bodySource;
    public K_ColorSrcMgr colorSource;
    public RectTransform mainUiGroup;

    public Material materialForStencil;
    public GameObject prefabVirtualBody;

    [Tooltip("the image on the canvas, showing the results.")]
    public RawImage imgDisplay;
    public RectTransform imgDisplayContainer;

    public bool smoothFilter;

    [Header("** DISPLAY FLAGS **")]
    [CtrlToggle(label = "Overlay")]
    public bool showOverlays3D = true;
    [CtrlToggle(label = "Stencil")]
    public bool showStencil = true;
    [CtrlToggle(label = "Face Points")]
    public bool showFaceJoints = true;
    [CtrlToggle(label = "3D Tracker")]
    public bool showFaceTrackers3D = true;
    [CtrlToggle(label = "Body Bones")]
    public bool showBones = true;

    [CtrlToggle(label = "DEBUG_STENCIL")]
    public bool DEBUG_STENCIL = false;

    [Space]
    [Space]
    [Header("** hierarchy **")]
    public AtributeDisplay factorsDisplay;
    public Camera virtualCamera;
    public MeshRenderer virtualProjection;
    private RenderTexture resultTexture;
    public Transform virtualBodiesRoot;
    public Transform overlayGroupsRoot;

    // -------------------------------------------------------------------
    private Dictionary<ulong, VirtualBody> _bodies = new Dictionary<ulong, VirtualBody>();
    private AspectRatioFitter compAspectRatio;

    public List<OverlayGroup> overlayGroups;

    public int currentGroupIdx;
    public OverlayGroup currentOverlaysGroup;
    private float currentGroupStartTime;
    public List<GameObject> currentAvailableOverlays;

    public static string groupsJsonFilename = "groups.json";
    private int defaultGroupTime = 120;

    public static string FactorsJsonFilename = "settings.json";

    public VirtualizerFactors factors { get; private set; }
    // -------------------------------------------------------------------
    // -------------------------------------------------------------------
    public BodiesVirtualizer()
    {
        factors = new VirtualizerFactors();
    }

    private void Awake()
    {
        //setup virtual camera
        resultTexture = new RenderTexture(K_consts.FRAME_COLOR_W, K_consts.FRAME_COLOR_H, 0, RenderTextureFormat.ARGB32);
        virtualCamera.targetTexture = resultTexture;
        imgDisplay.material.mainTexture = resultTexture;
        virtualCamera.fieldOfView = K_consts.CAMERA_FOV;

        //flip projection texture
        virtualProjection.material.SetTextureScale("_MainTex", new Vector2(-1, 1));

        //create AspectRatioFitter component for the result image (on canvas):
        if (imgDisplay != null)
        {
            compAspectRatio = imgDisplay.GetComponent<AspectRatioFitter>();
            if (compAspectRatio == null)
            {
                compAspectRatio = imgDisplay.gameObject.AddComponent<AspectRatioFitter>();
                compAspectRatio.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                compAspectRatio.aspectRatio = 1f * K_consts.FRAME_COLOR_W / K_consts.FRAME_COLOR_H;
            }
        }

        //get overlays:
        overlayGroups = new List<OverlayGroup>();
        var groupsCount = overlayGroupsRoot.childCount;
        for (int i_grp = 0; i_grp < groupsCount; i_grp++)
        {
            var grp = overlayGroupsRoot.GetChild(i_grp);
            if (!grp.gameObject.activeSelf)
                continue;

            var list = new List<GameObject>();
            for (int i_child = 0; i_child < grp.childCount; i_child++)
            {
                var child = grp.GetChild(i_child).gameObject;
                if (!child.gameObject.activeSelf)
                    continue;

                //only children with template script
                if (child.GetComponent<OverlayTemplate>() != null)
                {
                    list.Add(child);
                }
            }

            overlayGroups.Add(new OverlayGroup(grp.name, list));
        }

        //reset group count
        currentGroupIdx = -1;
        overlayGroupsRoot.gameObject.SetActive(false);

        //first group looping iteration.
        updateCurrentOverlayGroup();

        //remove all existing body objects if exist
        var childCount = virtualBodiesRoot.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(virtualBodiesRoot.GetChild(i).gameObject);
        }

        loadJson();

        factorsDisplay.objTarget = factors;
    }

    void Start()
    {
        currentGroupStartTime = Time.time;
    }

    private void OnDestroy()
    {
        if (resultTexture != null)
        {
            resultTexture.Release();
            Destroy(resultTexture);
            resultTexture = null;
        }

        saveJson();
    }

    // Update is called once per frame
    void Update()
    {
        //scale image:
        var s = factors.zoom;
        imgDisplayContainer.transform.localScale = new Vector3(s, s, s);
        imgDisplayContainer.offsetMin = new Vector2(factors.offsetX * Screen.width * s, factors.offsetY * Screen.height * s);
        imgDisplayContainer.offsetMax = new Vector2(+factors.offsetX * Screen.width * s, +factors.offsetY * Screen.height * s);



        //update stencil material
        Texture texture = colorSource.GetColorTexture();
        if (texture != null)
        {
            materialForStencil.mainTexture = texture;
            virtualProjection.material.mainTexture = texture;
        }

        if (DEBUG_STENCIL)
            materialForStencil.color = UnityEngine.Color.red;
        else
            materialForStencil.color = UnityEngine.Color.white;


        //handle groups looping
        updateCurrentOverlayGroup();

        handleBodies();

        //debug cycle:
        //switch group:
        if (Input.GetKeyDown(KeyCode.Space))
        {
            updateCurrentOverlayGroup(forceNext: true);

        }

        //switch active hats:
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            handleBodies(forceClear: true);
        }
    }

    // -------------------------------------------------------------------
    public void updateCurrentOverlayGroup(bool forceNext = false)
    {
        //time for next group?
        if (forceNext || currentOverlaysGroup == null || Time.time > currentGroupStartTime + currentOverlaysGroup.info.timeSeconds)
        {
            bool found = false;

            currentGroupStartTime = Time.time;
            currentAvailableOverlays.Clear();

            //first time
            if (currentOverlaysGroup == null)
                currentGroupIdx = -1;

            for (int i = 0; i < overlayGroups.Count; i++)
            {
                currentGroupIdx = (currentGroupIdx + 1) % overlayGroups.Count;
                currentOverlaysGroup = overlayGroups[currentGroupIdx];
                if (currentOverlaysGroup.info.enabled && currentOverlaysGroup.overlays.Count > 0)
                {
                    found = true;
                    Debug.Log("starting overlay group: " + currentOverlaysGroup.info.name);
                    break;
                }
            }

            if (!found)
            {
                currentGroupIdx = -1;
                currentOverlaysGroup = null;
                Debug.LogWarning("couldn't find an enabled groups with any available overlays");
            }

            //clear bodies
            handleBodies(forceClear: true);
        }
    }

    public GameObject getNextOverlay()
    {
        // get a random item from the current group. 
        // random order, without repeats, from a temp list (clone of the group items)
        // if all available items are used - refill list and repeat.

        if (currentOverlaysGroup == null)
        {
            Debug.LogWarning("no available overlay group");
            return null;
        }

        //first time
        if (currentAvailableOverlays == null)
            currentAvailableOverlays = new List<GameObject>();

        //re-fill available overlays if needed:
        if (currentAvailableOverlays.Count == 0)
        {
            foreach (var item in currentOverlaysGroup.overlays)
                currentAvailableOverlays.Add(item);
        }

        //get and remove a random item:
        var randomIdx = UnityEngine.Random.Range(0, currentAvailableOverlays.Count);
        var overlay = currentAvailableOverlays[randomIdx];
        currentAvailableOverlays.RemoveAt(randomIdx);

        return overlay;
    }

    private void handleBodies(bool forceClear = false)
    {
        //get data:        
        var faceData = bodySource.GetFaceData();
        var bodyData = bodySource.GetBodyDate();
        var coordMapper = bodySource.getCoordsMapper();

        //fake clear data:
        if (forceClear)
        {
            faceData = new FaceFrameResult[0];
            bodyData = new Windows.Kinect.Body[0];
        }

        if (bodyData == null)
            return;

        //collect current tracked bodies
        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in bodyData)
        {
            if (body == null)
                continue;

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        //remove untracked ids:
        List<ulong> knownIds = new List<ulong>(_bodies.Keys);
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                var body = _bodies[trackingId];
                _bodies.Remove(trackingId);
                Destroy(body.gameObject);
            }
        }

        //update / create
        for (int i = 0; i < bodyData.Length; i++)
        {
            var body = bodyData[i];
            var face = faceData[i];

            if (body != null && body.IsTracked)
            {
                if (!_bodies.ContainsKey(body.TrackingId))
                {
                    //create new body
                    var newBody = CreateBodyObject(body.TrackingId);
                    _bodies[body.TrackingId] = newBody;
                }
                else
                {
                    //update bodies
                    _bodies[body.TrackingId].RefreshBodyObject(coordMapper, body, face);
                }
            }
        }
    }

    private VirtualBody CreateBodyObject(ulong trackingId)
    {
        GameObject newBodyObj = GameObject.Instantiate(prefabVirtualBody);
        var newBody = newBodyObj.GetComponent<VirtualBody>();
        newBody.trackingId = trackingId;
        newBodyObj.transform.SetParent(virtualBodiesRoot, false);

        newBody.set3DOverlay(GameObject.Instantiate(getNextOverlay()));

        return newBody;
    }

    // -------------------------------------------------------------------
    #region JSON

    public void loadJson()
    {
        //groups 
        try
        {
            var path = Utils.getSettingsPath(groupsJsonFilename);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);

                var jsonGroups = JsonUtility.FromJson<OverlayGroupsJson>(json);

                //update from json:
                defaultGroupTime = jsonGroups.defaultTimeSeconds;
                if (overlayGroups != null)
                {
                    foreach (var grp in overlayGroups)
                    {
                        var match = jsonGroups.groups.Where(x => x.name.Trim().ToLower().Equals(grp.info.name.Trim().ToLower())).FirstOrDefault();
                        if (match != null)
                        {
                            grp.info.timeSeconds = match.timeSeconds;
                            grp.info.enabled = match.enabled;
                        }
                        else
                        {
                            grp.info.timeSeconds = jsonGroups.defaultTimeSeconds;
                            grp.info.enabled = true;
                        }
                    }
                }

                Debug.Log("reading groups JSON from " + path);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        //factors
        try
        {
            var path = Utils.getSettingsPath(FactorsJsonFilename);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);

                factors = JsonUtility.FromJson<VirtualizerFactors>(json);
                Debug.Log("reading factors JSON from " + path);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

    }

    public void saveJson()
    {
        //groups 
        try
        {
            var path = Utils.getSettingsPath(groupsJsonFilename);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            //create json:
            var jsonGroups = new OverlayGroupsJson();
            jsonGroups.defaultTimeSeconds = this.defaultGroupTime;
            jsonGroups.groups = new List<OverlayGroupInfo>();

            foreach (var item in overlayGroups)
                jsonGroups.groups.Add(item.info);

            var json = JsonUtility.ToJson(jsonGroups, true);
            File.WriteAllText(path, json);
            Debug.Log("saving groups JSON to " + path);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        //factors
        try
        {
            var path = Utils.getSettingsPath(FactorsJsonFilename);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            //create json:
            var json = JsonUtility.ToJson(factors, true);
            File.WriteAllText(path, json);
            Debug.Log("saving factors JSON to " + path);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

    }

    #endregion
}
