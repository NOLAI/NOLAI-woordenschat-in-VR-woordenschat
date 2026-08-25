/*
 * ThinkingAnimationManager - Manages thinking/loading animation
 *
 * This script controls thinking icon animations while the dashboard PC is processing the request:
 * - Scales icons between min and max values
 * - Creates pulsing breathing effect
 * - Can start/stop the animation
 * - Manages coroutine for smooth scaling
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThinkingAnimationManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> thinkingIcons;
    public float minScale = 0.7f;
    public float maxScale = 1.3f;
    public float scaleSpeed = 0.3f;
    private bool isScaling = false;
    private Coroutine scaleCoroutine;

    public void StartThinkingAnimation()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        foreach (GameObject icon in thinkingIcons)
        {
            icon.SetActive(true);
            icon.transform.localScale = new Vector3(minScale, minScale, minScale);
            icon.transform.localRotation = Quaternion.identity;
        }

        scaleCoroutine = StartCoroutine(ScaleAnimation());
    }

    public void StopThinkingAnimation()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        foreach (GameObject icon in thinkingIcons)
        {
            icon.SetActive(false);
            icon.transform.localScale = Vector3.one;
            icon.transform.localRotation = Quaternion.identity;
        }
    }

    IEnumerator ScaleAnimation()
    {
        bool scalingUp = true;

        while (true)
        {
            foreach (GameObject icon in thinkingIcons)
            {
                if (scalingUp)
                {
                    icon.transform.localScale += Vector3.one * scaleSpeed * Time.deltaTime;
                    if (icon.transform.localScale.x >= maxScale)
                        scalingUp = false;
                }
                else
                {
                    icon.transform.localScale -= Vector3.one * scaleSpeed * Time.deltaTime;
                    if (icon.transform.localScale.x <= minScale)
                        scalingUp = true;
                }
            }
            yield return null;
        }
    }
}
