using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour
{
    public void OnRollNotReady()
    {
        gameObject.SetActive(false);
    }

    public void OnRollReady()
    {
        gameObject.SetActive(true);
    }
}
