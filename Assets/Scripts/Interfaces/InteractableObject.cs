using System;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public GameObject selectVisual;
    private bool isInteractable;
    public bool IsInteractable
    {
        get => isInteractable;
        set
        {
            isInteractable = value;
            selectVisual.SetActive(value);
        }
    }
    public virtual void Interact() { }

    public void OnThingSelected() { if (IsInteractable) IsInteractable = false; }
}


