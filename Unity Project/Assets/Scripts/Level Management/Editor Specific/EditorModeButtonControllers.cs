/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * Level editor main logic & UI script.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class EditorModeButtonControllers : MonoSingleton<EditorModeButtonControllers> //MonoSingleton because i'm lazy
{
    [SerializeField] GameObject paintUiGroup;
    [SerializeField] GameObject eventUiGroup;

    [SerializeField] Dropdown levelSelection;
    [SerializeField] Text newLevelNameInput;
    [SerializeField] Text currentModeText;

    [SerializeField] Button AddTilesButton;
    [SerializeField] Button AddPropsButton;
    [SerializeField] Button DeleteButton;
    [SerializeField] Text DeleteBtnText;

    [SerializeField] Text CurrentMapNameInEditor;

    [SerializeField] Button QuitButton;

    [SerializeField] PropRotationSelector PropRotationPanel;
    [SerializeField] GameObject PropButtonNEW;

    [SerializeField] GameObject BrushComponentUI;
    [SerializeField] GameObject BrushComponentUI_OFFSET;
    [SerializeField] GameObject DraggableComponentUI;
    [SerializeField] GameObject DraggableComponentUI_OFFSET;

    [SerializeField] GameObject DynamicallyResizingContent_Props;
    [SerializeField] GameObject DynamicallyResizingContent_Tiles;

    [SerializeField] Camera saveCamera;
    [SerializeField] SpriteRenderer backgroundGrid;

    private Animator animatorComponent;

    List<int> levelIndexFixer = new List<int>();

    /* Set initial editor UI states */
    private void Start()
    {
#if UNITY_WEBGL
        Debug.LogError("LEVEL EDITING IS NOT SUPPORTED IN WEBGL");
#else

        //Get animator and setup UI
        animatorComponent = GetComponent<Animator>();
        animatorComponent.SetBool("showLoadscreenTransition", true);
        SwapMainState(MAIN_UI_STATE.PRE_EDIT_STATE);

        //Setup dynamic content fields
        int tileCount = 0;
        foreach (TileTypes tileType in Enum.GetValues(typeof(TileTypes)))
        {
            if (!TileProperties.IsVisibleInEditor(tileType)) continue;
            tileCount++;
        }
        float UI_SCALER = GetComponent<CanvasScaler>().referenceResolution.x / Screen.width;
        float x_offset = BrushComponentUI_OFFSET.transform.position.x - BrushComponentUI.transform.position.x;
        float pos_x = BrushComponentUI.transform.position.x;
        DynamicallyResizingContent_Tiles.GetComponent<RectTransform>().offsetMax =
            new Vector2(
                (tileCount * x_offset * UI_SCALER) - 1350,
                DynamicallyResizingContent_Tiles.GetComponent<RectTransform>().offsetMax.y
            );
        foreach (TileTypes tileType in Enum.GetValues(typeof(TileTypes)))
        {
            if (!TileProperties.IsVisibleInEditor(tileType)) continue;
            GameObject newBrush = Instantiate(BrushComponentUI, new Vector3(pos_x, BrushComponentUI.transform.position.y), Quaternion.identity) as GameObject;
            newBrush.transform.SetParent(BrushComponentUI.transform.parent);
            newBrush.GetComponent<BrushEditorComponent>().objectType = tileType;
            newBrush.SetActive(true);
            newBrush.transform.localScale = new Vector3(1, 1, 1);
            pos_x += x_offset;
        }
        int propCount = 0;
        foreach (PropTypes propType in Enum.GetValues(typeof(PropTypes)))
        {
            if (!PropProperties.IsVisibleInEditor(propType)) continue;
            propCount++;
        }
        pos_x = DraggableComponentUI.transform.position.x;
        DynamicallyResizingContent_Props.GetComponent<RectTransform>().offsetMax =
            new Vector2(
                (propCount * x_offset * UI_SCALER) - 1350,
                DynamicallyResizingContent_Props.GetComponent<RectTransform>().offsetMax.y
            );
        foreach (PropTypes propType in Enum.GetValues(typeof(PropTypes)))
        {
            if (!PropProperties.IsVisibleInEditor(propType)) continue;
            GameObject newDraggable = Instantiate(PropButtonNEW, new Vector3(pos_x, DraggableComponentUI.transform.position.y), Quaternion.identity) as GameObject;
            newDraggable.transform.SetParent(DraggableComponentUI.transform.parent);
            newDraggable.GetComponent<PropButton>().SetPropType(propType);
            newDraggable.SetActive(true);
            newDraggable.transform.localScale = new Vector3(1, 1, 1);
            pos_x += x_offset;
        }
        PropRotationSelector.Instance.Hide();
#endif
    }

    /* User selected to create a new level */
    public void CreateNewLevel()
    {
        //Validate the level name first
        bool level_is_valid = true;
        foreach (LevelMetadata level in LevelManager.Instance.LevelMetadata)
        {
            if (level.levelName == newLevelNameInput.text)
            {
                level_is_valid = false;
                break;
            }
        }
        var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
        if (newLevelNameInput.text == "" || !regexItem.IsMatch(newLevelNameInput.text)) level_is_valid = false;
        if (!level_is_valid)
        {
            ConfirmationPopup.Instance.Show("Please enter a unique and valid level name.");
            return;
        }

        //Create the new instance and go into editor
        StartCoroutine(LevelManager.Instance.Load(LevelEditor.Instance.CreateLevel(newLevelNameInput.text)));
        OpenEditor(newLevelNameInput.text, false);
    }

    /* User selected to load an existing level */
    public void LoadSelectedLevel()
    {
        //Load the instance and go into editor
        StartCoroutine(LevelManager.Instance.Load(levelIndexFixer[levelSelection.value]));
        OpenEditor(LevelManager.Instance.CurrentMeta.levelName);
    }

    /* Go into the level editor */
    private void OpenEditor(string mapName, bool allowQuit = true)
    {
        animatorComponent.SetBool("showLoadscreenTransition", false);
        SwapMainState(MAIN_UI_STATE.MAIN_EDIT_STATE);

        paintUiGroup.SetActive(false);
        eventUiGroup.SetActive(false);
        QuitButton.interactable = allowQuit;

        LevelEditor.Instance.SetEditorMode(LevelEditorMode.VOID_MODE);
        LevelEditor.Instance.SetActiveBrush((TileTypes)0);
        SwapInteraction(EDIT_MODE_UI_STATE.ALLOW_ALL_BUT_DELETE);
        DeleteBtnText.text = " Delete";

        currentModeText.text = "Current mode: " + LevelEditor.Instance.Mode.ToString();
        CurrentMapNameInEditor.text = mapName;
    }

    /* Enable tile painting mode */
    public void SetPaintMode()
    {
        LevelEditor.Instance.SetEditorMode(LevelEditorMode.PAINTING_TILES);
        StartCoroutine(SwapContentTray(EDIT_MODE_UI_STATE.ALLOW_ALL_BUT_TILES));
        DeleteBtnText.text = "Delete Tiles";
    }

    /* Enable prop placement mode */
    public void SetEventMode()
    {
        LevelEditor.Instance.SetEditorMode(LevelEditorMode.DRAGGING_PROPS);
        StartCoroutine(SwapContentTray(EDIT_MODE_UI_STATE.ALLOW_ALL_BUT_PROPS));
        DeleteBtnText.text = "Delete Props";
    }

    /* Enable delete mode for current editor mode (prop/tile) */
    public void SetDeleteMode()
    {
        if (((int)LevelEditor.Instance.Mode + 1) % 2 == 0) return;
        LevelEditor.Instance.SetEditorMode(LevelEditor.Instance.Mode + 1);
        StartCoroutine(SwapContentTray(EDIT_MODE_UI_STATE.ALLOW_ALL_BUT_DELETE, false));
    }

    /* Show/hide content tray for new state (todo: remove EDIT_MODE_UI_STATE in favour of working just from LevelEditorMode) */
    private IEnumerator SwapContentTray(EDIT_MODE_UI_STATE ui_state, bool reshow_tray = true)
    {
        currentModeText.text = "Current mode: " + LevelEditor.Instance.Mode.ToString();

        PropRotationSelector.Instance.Hide();
        SwapInteraction(EDIT_MODE_UI_STATE.ALLOW_NONE);

        animatorComponent.SetBool("showContentPanel", false);
        yield return new WaitForSeconds(0.5f);
        paintUiGroup.SetActive(LevelEditor.Instance.Mode == LevelEditorMode.PAINTING_TILES);
        eventUiGroup.SetActive(LevelEditor.Instance.Mode == LevelEditorMode.DRAGGING_PROPS);
        yield return new WaitForSeconds(0.5f);
        if (reshow_tray) animatorComponent.SetBool("showContentPanel", true);

        yield return new WaitForSeconds(0.5f);
        SwapInteraction(ui_state);
    }

    /* Quit the editor without saving */
    public void QuitEditor()
    {
        ConfirmationPopup.Instance.Show("Are you sure you want to exit without saving?", "Yes", true, "No");
        StartCoroutine(QuitEditorConfirmation());
    }
    private IEnumerator QuitEditorConfirmation()
    {
        while (ConfirmationPopup.Instance.Selection == PopupResult.NOT_SELECTED) yield return new WaitForEndOfFrame();
        if (ConfirmationPopup.Instance.Selection != PopupResult.SELECTED_OK) yield break;

        ExitEditor();
        ConfirmationPopup.Instance.Show("Exited without saving.");
    }

    /* Quit the editor and save */
    public void SaveLevel()
    {
        ConfirmationPopup.Instance.Show("Are you sure you want to save?", "Yes", true, "No");
        StartCoroutine(SaveLevelConfirmation());
    }
    private IEnumerator SaveLevelConfirmation()
    {
        while (ConfirmationPopup.Instance.Selection == PopupResult.NOT_SELECTED) yield return new WaitForEndOfFrame();
        if (ConfirmationPopup.Instance.Selection != PopupResult.SELECTED_OK) yield break;

        //Screenshot the level
        saveCamera.enabled = true;
        backgroundGrid.enabled = false;
        RenderTexture rt = new RenderTexture(1920, 1080, 24);
        saveCamera.targetTexture = rt;
        Texture2D screenshot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        saveCamera.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        saveCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        byte[] levelScreenshot = screenshot.EncodeToPNG();
        backgroundGrid.enabled = true;
        saveCamera.enabled = false;

        //Save the level
        LevelEditor.Instance.SaveLevel(levelScreenshot, 1, 0, new AbilityCounts(1,1,1,1)); //todo: remove ability info & hazard count
        ExitEditor();

        StartCoroutine(LevelManager.Instance.RefreshLevelData());
        bool test = false; bool test2 = false; //do not remove this line
        while (!LevelManager.Instance.LevelMetaUpdated) yield return new WaitForEndOfFrame();

        ConfirmationPopup.Instance.Show("Saved custom map!");
    }

    /* Exit the editor state and return to initial state */
    private void ExitEditor()
    {
        StartCoroutine(ExitEditorAnim());
    }
    private IEnumerator ExitEditorAnim()
    {
        LevelEditor.Instance.SetEditorMode((LevelEditorMode)0);
        PropRotationSelector.Instance.Hide();
        SwapMainState(MAIN_UI_STATE.PRE_EDIT_STATE);

        yield return new WaitForSeconds(0.5f);
        LevelManager.Instance.Unload();
    }

    /* Exit editor entirely and return to main menu */
    public void ReturnToFrontend()
    {
        ConfirmationPopup.Instance.Hide();
        animatorComponent.SetBool("isReturningToFrontend", true);
        animatorComponent.SetBool("showLoadscreenTransition", true);
        animatorComponent.SetBool("showInitialUI", false);
    }
    protected void BackToFrontendLogic()
    {
        SceneHandler.Instance.LoadScene("MainMenu");
    }

    /* Update the levels available in the selection dropdown */
    private void UpdateLevelDropdown()
    {
        levelSelection.options.Clear();
        levelIndexFixer.Clear();
        foreach (LevelMetadata levelData in LevelManager.Instance.LevelMetadata)
        {
#if UNITY_EDITOR
#else
            if (levelData.isCampaignLevel) continue;
#endif
            levelSelection.options.Add(new Dropdown.OptionData(levelData.levelName));
            levelIndexFixer.Add(levelData.levelGUID);
        }
        levelSelection.RefreshShownValue();
        newLevelNameInput.text = "";
    }

    /* Set edit mode UI states */
    private enum EDIT_MODE_UI_STATE
    {
        ALLOW_ALL_BUT_TILES,
        ALLOW_ALL_BUT_PROPS,
        ALLOW_ALL_BUT_SCRIPTED,
        ALLOW_ALL_BUT_SPAWNERS,
        ALLOW_ALL_BUT_DELETE,
        ALLOW_ALL,
        ALLOW_NONE,
    }
    private void SwapInteraction(EDIT_MODE_UI_STATE ui_state)
    {
        AddTilesButton.interactable = !(ui_state == EDIT_MODE_UI_STATE.ALLOW_NONE || ui_state == EDIT_MODE_UI_STATE.ALLOW_ALL_BUT_TILES);
        AddPropsButton.interactable = !(ui_state == EDIT_MODE_UI_STATE.ALLOW_NONE || ui_state == EDIT_MODE_UI_STATE.ALLOW_ALL_BUT_PROPS);
        DeleteButton.interactable = !(ui_state == EDIT_MODE_UI_STATE.ALLOW_NONE || ui_state == EDIT_MODE_UI_STATE.ALLOW_ALL_BUT_DELETE);
    }

    /* Set main UI state */
    private enum MAIN_UI_STATE
    {
        PRE_EDIT_STATE,
        MAIN_EDIT_STATE,
    }
    private void SwapMainState(MAIN_UI_STATE ui_state)
    {
        animatorComponent.SetBool("showContentPanel", false);

        switch (ui_state)
        {
            case MAIN_UI_STATE.PRE_EDIT_STATE:
                UpdateLevelDropdown();
                animatorComponent.SetBool("showInitialUI", true);
                animatorComponent.SetBool("showSaveButton", false);
                animatorComponent.SetBool("showEditorTools", false);
                animatorComponent.SetBool("isReturningToFrontend", false);
                break;
            case MAIN_UI_STATE.MAIN_EDIT_STATE:
                animatorComponent.SetBool("showInitialUI", false);
                animatorComponent.SetBool("showSaveButton", true);
                animatorComponent.SetBool("showEditorTools", true);
                break;
        }
    }
}