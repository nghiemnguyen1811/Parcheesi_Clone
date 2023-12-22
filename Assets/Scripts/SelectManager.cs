using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if(hit.transform.parent == null) return;
                if (hit.transform.parent.TryGetComponent<InteractableObject>(out InteractableObject interactable))
                {  
                    if(interactable.IsInteractable)
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }
}
