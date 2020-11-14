/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * This script allows you to hook scripts to prop types, allowing for bespoke logic.
 * To add a new bespoke prop script, navigate down to the commented bit below.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This is a manager held in the scene which keeps gameobject references for bespoke scripted prop implementations */
public class BespokePropManager : MonoSingleton<BespokePropManager>
{
    //Door config
    [SerializeField] float openTime = 3.0f;
    public float OpenTime { get { return openTime; } }

    //Alert Boxes
    [SerializeField] public GameObject alertPrefab;
}
