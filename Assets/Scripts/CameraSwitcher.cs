using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraSwitcher : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = this.GetComponent<Animator>();
    }
    public void SwitchCameraTo(int index)
    {
        switch(index)
        {
            case 0:
            animator.Play("CM vcam1");
            break;
            case 1:
            animator.Play("CM vcam2");
            break;
            case 2:
            animator.Play("CM vcam3");
            break;
            case 3:
            animator.Play("CM vcam4");
            break;
        }
    }
}
