/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * New prop button that opens the rotation panel in level editor (part of the playwest rework).
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropButton : MonoBehaviour
{
    private PropTypes propType;

    /* Set prop type */
    public void SetPropType(PropTypes _type)
    {
        propType = _type;
        gameObject.GetComponent<Image>().sprite = PropProperties.GetSpriteSet(propType).editorUISprite;
    }

    /* Show rotation panel on click */
    public void ShowInRotationPanel()
    {
        PropRotationSelector.Instance.SetPropType(propType);
        PropRotationSelector.Instance.Show();
    }
}
