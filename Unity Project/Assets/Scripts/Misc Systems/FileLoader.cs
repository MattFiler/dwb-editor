/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * The core file loading system - to be used by ALL scripts.
 * This works across local and WebGL builds, and is ASYNCHRONOUS.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class FileLoader : MonoSingleton<FileLoader>
{
    /* Asynchronously load a file as a string */
    public IEnumerator LoadAsText(string filePath, Action<string> callback)
    {
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            WWW www = new WWW(filePath);
            yield return www;

            while (!www.isDone)
                yield return new WaitForEndOfFrame();

            callback(www.text);
        }
        else
        {
            callback(File.ReadAllText(filePath));
        }
    }

    /* Asynchronously load a file as a byte array */
    public IEnumerator LoadAsBytes(string filePath, Action<byte[]> callback)
    {
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            WWW www = new WWW(filePath);
            yield return www;

            while (!www.isDone)
                yield return new WaitForEndOfFrame();

            callback(www.bytes);
        }
        else
        {
            callback(File.ReadAllBytes(filePath));
        }
    }
}
