/*
 * CircularProgressBar - Visual timer display using radial progress bar
 *
 * This script creates a circular countdown timer:
 * - Uses Unity's UI Image component in radial mode
 * - Fills counter-clockwise from top
 * - Animates fill amount during countdown
 * - Auto-completes when timer reaches zero
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CircularProgressBar : MonoBehaviour
{
    private Image radialProgressBar;
    private float currentTime;
    private float maxTime;
    private bool isCountingDown;

    private void Awake()
    {
        radialProgressBar = GetComponent<Image>();
        radialProgressBar.fillMethod = Image.FillMethod.Radial360;
        radialProgressBar.fillOrigin = (int)Image.Origin360.Top;
        radialProgressBar.fillAmount = 0f; // Start leeg
    }

    private void Update()
    {
        if (isCountingDown)
        {
            currentTime -= Time.deltaTime;
            // Bereken hoeveel er al is gevuld: 1 - (currentTime / maxTime)
            radialProgressBar.fillAmount = 1f - Mathf.Clamp01(currentTime / maxTime);

            if (currentTime <= 0f)
            {
                StopCountdown();
            }
        }
    }

    public void StartCountdown(float duration)
    {
        if (duration <= 0f)
        {
            Debug.LogWarning("Countdown duration must be greater than 0.");
            return;
        }

        maxTime = duration;
        currentTime = maxTime;
        isCountingDown = true;
        radialProgressBar.fillAmount = 0f;
    }

    public void StopCountdown()
    {
        isCountingDown = false;
        radialProgressBar.fillAmount = 1f;
    }
}

