/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * An object within the level editor that can be dragged into the world.
 * LevelTileEntity should be used for actual in-world objects.
 * 
 * This is for BRUSH tile types only (non-events: e.g. a wall).
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushEditorComponent : MonoBehaviour
{
    public TileTypes objectType;

    /* Setup component on start */
    private void Start()
    {
        if (!gameObject.GetComponent<BoxCollider2D>()) gameObject.AddComponent<BoxCollider2D>();
        if (!gameObject.GetComponent<Image>()) gameObject.AddComponent<Image>();

        gameObject.GetComponent<Image>().sprite = TileProperties.GetSpriteSet(objectType).editorUI;

        gameObject.layer = 8; //Must match TouchManager
    }

    /* When selected, update the brush type to ours */
    public void OnSelect()
    {
        LevelEditor.Instance.SetActiveBrush(objectType);
    }
}
