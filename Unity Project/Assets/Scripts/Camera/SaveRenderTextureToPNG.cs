using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SaveRenderTextureToPNG : MonoBehaviour
{
    private RenderTexture renderTexture;
    [SerializeField] private Vector2Int renderTextureDims = new Vector2Int(500, 800);

    private void Awake()
    {
        renderTexture = GetComponent<Camera>().targetTexture;
    }

    private void LateUpdate()
    {
        if (Input.GetKeyUp(KeyCode.AltGr))
        {
            StartCoroutine(Save());
        }
    }

    private IEnumerator Save()
    {
        renderTexture.Release();
        yield return null;

        RenderTexture.active = renderTexture;

        Texture2D texture = new Texture2D(renderTextureDims.x, renderTextureDims.y, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, renderTextureDims.x, renderTextureDims.y), 0, 0);
        //RenderTexture.active = null;

        byte[] bytes = texture.EncodeToPNG();

        string dateTime = System.DateTime.Now.Day + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Hour + "-" + System.DateTime.Now.Minute + "-" + System.DateTime.Now.Second;
        string path = Application.dataPath + "/Resources/RenderTextureCaptures/" + dateTime + ".png";
        File.WriteAllBytes(path, bytes);

        Debug.Log("Saved render texture to PNG... file path: " + path);
    }
}
