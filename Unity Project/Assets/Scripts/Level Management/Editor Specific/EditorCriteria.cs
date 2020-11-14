/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * A script to control a critera checkbox in the editor UI.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorCriteria : MonoBehaviour
{
    [SerializeField] Text criteriaText;
    public void SetText(string text)
    {
        criteriaText.text = text;
    }

    [SerializeField] GameObject criteriaChecked;
    [SerializeField] GameObject criteriaUnchecked;
    bool isComplete = true;
    public void SetCriteraComplete(bool complete = true)
    {
        isComplete = complete;
        criteriaChecked.SetActive(complete);
        criteriaUnchecked.SetActive(!complete);
    }
    public bool GetCriteriaComplete()
    {
        return isComplete;
    }
}
