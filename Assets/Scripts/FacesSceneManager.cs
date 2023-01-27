using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class FacesSceneManager : MonoBehaviour
{
    // -------------------------------------------------------------------
    // SINGLETON
    // -------------------------------------------------------------------
    private static FacesSceneManager _instance;
    private static object _lock = new object();

    public static FacesSceneManager Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<FacesSceneManager>();

                    if (_instance == null)
                    {
                        Debug.LogError("can't find singleton FacesSceneManager");
                    }
                }
                return _instance;
            }
        }
    }

    // -------------------------------------------------------------------
    // LIFE
    // -------------------------------------------------------------------
    public FacesSceneManager()
    {
    }

    private void Awake()
    {
       
    }

   

    // Update is called once per frame
    void Update()
    {
    
    }

    public void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
         Application.OpenURL(webplayerQuitURL);
#else
         Application.Quit();
#endif
    }
}
