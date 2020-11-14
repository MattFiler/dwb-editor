using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiPopup : MonoBehaviour
{
    Animator animator = null;
    bool active = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Popup()
    {
        if (animator != null)
        {
            active = !active;
            animator.SetTrigger(active ? "Enter" : "Exit");
        }
        else gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    public void Disable()
    {
        if (animator != null)
        {
            if (!active) return;
            active = false;
            animator.SetTrigger("Exit");
        }
        else gameObject.SetActive(false);
    }
}
