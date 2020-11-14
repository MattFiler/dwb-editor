/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * Debug ability to move the game camera left/right/up/down.
 * Just a quick implementation that should be improved/replaced down the line.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCameraMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1.5f;
    private Camera cam;
    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.W)) cam.transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position + new Vector3(0, moveSpeed, 0), Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) cam.transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position + new Vector3(-moveSpeed, 0, 0), Time.deltaTime);
        if (Input.GetKey(KeyCode.S)) cam.transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position + new Vector3(0, -moveSpeed, 0), Time.deltaTime);
        if (Input.GetKey(KeyCode.D)) cam.transform.position = Vector3.Lerp(cam.transform.position, cam.transform.position + new Vector3(moveSpeed, 0, 0), Time.deltaTime);
    }
}
