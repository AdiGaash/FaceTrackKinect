using Microsoft.Kinect.Face;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;

public static class Utils
{
    /// <summary>
    /// returns true if the user is currently in a focused InputField <para/>
    /// good for testing before listening to key events...!
    /// </summary>
    /// <returns></returns>
    public static bool isCurrentlyEditingText()
    {
        if (EventSystem.current != null)
        {
            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            if (currentSelectedGameObject != null)
            {
                var inputField = currentSelectedGameObject.GetComponent<InputField>();
                if (inputField != null)
                {
                    return inputField.isFocused;
                }
            }
        }

        return false;
    }

    public static long currentTimeMills
    {
        get
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }

    public static string getStrearmingPath(string filePath)
    {
        return Application.streamingAssetsPath + Path.AltDirectorySeparatorChar + filePath;
    }

    // -------------------------------------------------------------------
    public static string getDataPath()
    {
        //return Application.persistentDataPath;

        var path = Application.dataPath;
        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            path += "/../../";
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            path += "/../";
        }
        return path;
    }

    public static string getSettingsPath(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "";

        var dataPath = getDataPath();

        var rootDir = Path.Combine(dataPath, "settings");

        var dir = Path.GetDirectoryName(fileName);
        var filenameNoExt = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext))
            ext = ".json";
        var re = Path.Combine(dir, filenameNoExt + ext);

        return Path.Combine(rootDir, re);
    }
    /*
    public static string getJsonPath(string filename_)
    {
        if (string.IsNullOrEmpty(filename_))
            return "";

        var rootDir = Path.Combine(Application.persistentDataPath, "JSON");

        var dir = Path.GetDirectoryName(filename_);
        var filename = Path.GetFileNameWithoutExtension(filename_);
        var ext = Path.GetExtension(filename_);
        if (string.IsNullOrEmpty(ext))
            ext = ".json";
        var re = Path.Combine(dir, filename + ext);

        return Path.Combine(rootDir, re);
    }
    */
}

