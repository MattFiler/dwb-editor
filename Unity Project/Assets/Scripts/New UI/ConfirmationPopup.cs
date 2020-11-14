/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * A confirmation popup with YES/NO return types depending on user button selection.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopup : MonoSingleton<ConfirmationPopup>
{
    private Animator animatorComponent;
    [SerializeField] Text popupText;
    [SerializeField] GameObject notOkButton;
    [SerializeField] Text okBtnText;
    [SerializeField] Text notOkBtnText;

    private PopupResult popupUserSelection = PopupResult.NOT_SELECTED;
    public PopupResult Selection { get { return popupUserSelection; } }

    bool popupIsShownOrAnimating = false;

    /* Get animator for popup */
    private void Start()
    {
        animatorComponent = GetComponent<Animator>();
    }

    /* Show popup */
    public void Show(string main_message, string ok_btn_text = "OK", bool show_not_ok_button = false, string not_ok_button_text = "No")
    {
        StartCoroutine(WaitForPopupCloseToReshow(main_message, ok_btn_text, show_not_ok_button, not_ok_button_text));
    }
    private IEnumerator WaitForPopupCloseToReshow(string main_message, string ok_btn_text, bool show_not_ok_button, string not_ok_button_text)
    {
        while (popupIsShownOrAnimating)
        {
            yield return new WaitForEndOfFrame();
        }

        popupUserSelection = PopupResult.NOT_SELECTED;
        popupText.text = main_message;
        okBtnText.text = ok_btn_text;
        notOkBtnText.text = not_ok_button_text;
        notOkButton.SetActive(show_not_ok_button);
        animatorComponent.SetBool("showPopup", true);
        popupIsShownOrAnimating = true;
    }

    /* Hide popup */
    public void Hide()
    {
        animatorComponent.SetBool("showPopup", false);
    }
    protected void PopupHasHidden()
    {
        popupIsShownOrAnimating = false;
    }

    /* Popup UI interactions for OK / not OK */
    public void PopupOKBTN()
    {
        popupUserSelection = PopupResult.SELECTED_OK;
        animatorComponent.SetBool("showPopup", false);
    }
    public void PopupNOTOKBTN()
    {
        popupUserSelection = PopupResult.SELECTED_NOT_OK;
        animatorComponent.SetBool("showPopup", false);
    }
}

public enum PopupResult
{
    NOT_SELECTED,
    SELECTED_OK,
    SELECTED_NOT_OK,
}