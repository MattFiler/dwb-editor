using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu("UI/KeyboardButton", 30)]
public class KeyboardButton : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] Color defaultColour = Color.white;
    [SerializeField] Color highlightedColour = Color.gray;
    [SerializeField] Color pressedColour = new Color(0.4f, 0.4f, 0.4f, 1);
    [SerializeField] Image targetGraphic = null;

    [Header("Press Event")]
    [SerializeField] List<KeyCode> keys = new List<KeyCode>();
    [Tooltip("Does the Highlight() have to be called for the button to be pressed?")]
    [SerializeField] bool highlightedFirst = true;
    [Serializable] public class ButtonPressedEvent : UnityEvent { }
    [FormerlySerializedAs("onPress")]
    [SerializeField] private ButtonPressedEvent onPress = new ButtonPressedEvent();
    public ButtonPressedEvent pressEvent
    {
        get { return onPress; }
        set { onPress = value; }
    }

    public delegate void KeyboardButtonEvent();
    public KeyboardButtonEvent buttonHighlighted;
    public KeyboardButtonEvent buttonUnhighlighted;
    public KeyboardButtonEvent buttonPressed;
    public KeyboardButtonEvent buttonReleased;

    bool highlighted = false;
    bool pressed = false;
    KeyCode currentKey = KeyCode.None;

    public void Highlight()
    {
        if (highlighted) return;
        targetGraphic.color = highlightedColour;
        buttonHighlighted?.Invoke();
        highlighted = true;
    }

    public void Unhighlight()
    {
        if (!highlighted) return;
        targetGraphic.color = defaultColour;
        buttonUnhighlighted?.Invoke();
        highlighted = false;
    }

    private void Update()
    {
        ButtonInput();
    }

    private void ButtonInput()
    {
        if (Input.anyKeyDown) GetCurrentKey();
        if (currentKey == KeyCode.None) return;

        if (Input.GetKeyDown(currentKey)) Press();
        else if (Input.GetKeyUp(currentKey)) Release();
    }

    void GetCurrentKey()
    {
        if (currentKey != KeyCode.None) return;

        foreach(KeyCode key in keys)
        {
            if (Input.GetKeyDown(key))
            {
                currentKey = key;
                break;
            }
        }
    }

    private void Press()
    {
        if ((highlightedFirst && !highlighted) || pressed) return;
        pressed = true;
        targetGraphic.color = pressedColour;
        buttonPressed?.Invoke();
        onPress?.Invoke();
    }

    private void Release()
    {
        if ((highlightedFirst && !highlighted) || !pressed) return;
        targetGraphic.color = highlightedFirst ? highlightedColour : defaultColour;
        buttonReleased?.Invoke();
        currentKey = KeyCode.None;
        pressed = false;
    }

    private void OnDisable()
    {
        pressed = false;
        targetGraphic.color = defaultColour;
    }
}
