/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * New level editor prop rotation popup as part of the multi-tile prop rework.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropRotationSelector : MonoSingleton<PropRotationSelector>
{
    [SerializeField] GameObject DraggableComponentUI;

    [SerializeField] Text propDesc;

    [SerializeField] private DraggableEditorComponent frontBrush;
    [SerializeField] private DraggableEditorComponent leftBrush;
    [SerializeField] private DraggableEditorComponent backBrush;
    [SerializeField] private DraggableEditorComponent rightBrush;

    /* Set the prop type for the rotation selector */
    public void SetPropType(PropTypes _type)
    {
        propDesc.text = PropProperties.GetDescription(_type);

        frontBrush.Initialise(_type, PropRotation.FACING_FRONT);
        leftBrush.Initialise(_type, PropRotation.FACING_LEFT);
        backBrush.Initialise(_type, PropRotation.FACING_BACK);
        rightBrush.Initialise(_type, PropRotation.FACING_RIGHT);
    }

    /* Show/hide */
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
