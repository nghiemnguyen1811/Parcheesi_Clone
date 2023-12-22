using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : InteractableObject
{
    [SerializeField] private VoidEvent OnFieldInteract;
    public override void Interact()
    {
        OnFieldInteract.Raise();
    }
}
