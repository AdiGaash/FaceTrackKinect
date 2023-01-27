using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DisplayConfig : MonoBehaviour
{
    public static string jsonPath = "displayConfig.json";
    public bool isOn = true;

    public RectTransform UiContainerAll;

    private bool UiIsDisplayed = false;
    private DisplayConfigJSON config;

    private int wRequest;
    private int hRequest;
    private bool fullscreenRequest;
    private int qualityLevelRequest;
    private bool hideCursorRequest;

    private void Awake()
    {
        UiIsDisplayed = true;

        loadJson();

        wRequest = Screen.currentResolution.width;
        hRequest = Screen.currentResolution.height;
        fullscreenRequest = Screen.fullScreen;
        qualityLevelRequest = QualitySettings.GetQualityLevel();
        hideCursorRequest = false;

        if (!Application.isEditor)
        {
            //ui hidden in real app
            UiIsDisplayed = false;

            Debug.Log(String.Format("current resolution: {0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height));

            //get json requests
            if (config.hideCursor)
            {
                hideCursorRequest = true;
            }

            if (config.forceFullScreen)
            {
                fullscreenRequest = true;
            }

            if (config.forceResolution)
            {
                wRequest = config.forceRsolutionWidth;
                hRequest = config.forceRsolutionHeight;
            }

            if (config.forceFantastic)
            {
                qualityLevelRequest = 5;//fantastic
            }

            //get command line overrides

            var args = CommandLineArgsHelper.getArgsDictionary();
            foreach (var arg in args)
            {
                var key = arg.Key;
                var extras = arg.Value;

                switch (key.ToLower().Trim())
                {
                    case "debug":
                        {
                            var v = (extras.Count == 0 || CommandLineArgsHelper.getExtraAsBool(extras[0]));
                            hideCursorRequest = !v;
                            UiIsDisplayed = v;
                        }
                        break;

                    case "fullscreen":
                        fullscreenRequest = extras.Count == 0 || CommandLineArgsHelper.getExtraAsBool(extras[0]);
                        break;

                    case "width":
                        if (extras.Count == 1)
                            int.TryParse(extras[0], out wRequest);
                        break;

                    case "height":
                        if (extras.Count == 1)
                            int.TryParse(extras[0], out hRequest);
                        break;
                }
            }

            //fix settings 

            if (
                    //(config.forceFullScreen || config.forceResolution) &&
                    (Screen.fullScreen != fullscreenRequest || Screen.currentResolution.width != wRequest || Screen.currentResolution.height != hRequest)
                )
            {
                Debug.LogWarning("strted in wrong resolution - correcting");
                Screen.SetResolution(wRequest, hRequest, fullscreenRequest);
            }

            if (QualitySettings.GetQualityLevel() != qualityLevelRequest)
            {
                QualitySettings.SetQualityLevel(qualityLevelRequest, true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // "d" for debug
        if (Input.GetKeyDown(KeyCode.D))
        {
            UiIsDisplayed = !UiIsDisplayed;
        }

        UiContainerAll.gameObject.SetActive(UiIsDisplayed);

        //cursorVisibility:        
        if (isOn && hideCursorRequest)
            Cursor.visible = UiIsDisplayed || Application.isEditor;
    }

    public void loadJson()
    {
        var path = Utils.getSettingsPath(jsonPath);
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                config = JsonUtility.FromJson<DisplayConfigJSON>(json);
                Debug.Log("reading groups JSON from " + path);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        if (config == null)
        {
            config = new DisplayConfigJSON();
            saveJson();
        }
    }

    public void saveJson()
    {
        try
        {
            var path = Utils.getSettingsPath(jsonPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var json = JsonUtility.ToJson(config, true);
            File.WriteAllText(path, json);
            Debug.Log("saving JSON to " + path);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}