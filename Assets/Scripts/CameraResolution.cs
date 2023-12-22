using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    [SerializeField]
    private float widthRatio;

    [SerializeField]
    private float heightRatio;
    private void Awake()
    {
        Camera camera = GetComponent<Camera>();

        Rect rect = camera.rect;
        float scaleHeight = ((float)Screen.width / Screen.height) / ((float)widthRatio / heightRatio);
        float scalewidth = 1f / scaleHeight;
        if(scaleHeight < 1)
        {
            rect.height = scaleHeight;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else
        {
            rect.width = scalewidth;
            rect.x = (1f - scalewidth) / 2f;
        }

        camera.rect = rect;
    }
}
