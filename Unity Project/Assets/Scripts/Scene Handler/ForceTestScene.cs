/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * A script to auto load a level by GUID in the gameplay scene if one hasn't already been called.
 * Prevents errors loading into the gameplay scene in editor without a level instance.
 * 
 * AUTHORS:
 *  - Matt Filer
 *  - Alex Allman
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceTestScene : MonoBehaviour
{
    private bool _firstTickDone = false;
    [SerializeField] private int levelGUID = 59;
    private bool emergencyStop = false;

    private void LateUpdate()
    {
        if (!LevelManager.Instance.LoadedInDebugMode)
        {
            emergencyStop = true;
            return;
        }
        if (emergencyStop) return;
        if (_firstTickDone) return;
        _firstTickDone = true;
        if (!LevelManager.Instance.CurrentMeta.isLoaded) 
        { 
            Debug.LogWarning("Forcing scene load for testing");
            StartCoroutine(LevelManager.Instance.Load(levelGUID));
        }
    }
}
