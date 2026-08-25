/*
 * StatusEffectsManager - Manages status effect timers (e.g., microphone time limit)
 *
 * This script handles temporary status effects:
 * - Shows/hides progress bar and icon
 * - Counts down using CircularProgressBar
 * - Auto-hides when timer expires
 * - Can stop existing coroutines to prevent overlap
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StatusEffectsManager : MonoBehaviour
{
    [SerializeField] private GameObject progressBar; // Wijs in de Inspector toe aan het GameObject met CircularProgressBar
    [SerializeField] private GameObject MicrophoneIcon;
    private CircularProgressBar circularProgressBar;
    private bool isActive;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (progressBar != null)
        {
            circularProgressBar = progressBar.GetComponent<CircularProgressBar>();
        }
        else
        {
            Debug.LogError("ProgressBar GameObject is not assigned!");
        }
    }

    public void StartProgressBar(float duration)
    {
        if (circularProgressBar == null || MicrophoneIcon == null)
        {
            Debug.LogError("CircularProgressBar or MicrophoneIcon component not found!");
            return;
        }

        if (isActive && activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        isActive = true;
        progressBar.SetActive(true);
        MicrophoneIcon.SetActive(true);
        circularProgressBar.StartCountdown(duration);
        activeCoroutine = StartCoroutine(EndProgressBar(duration));
    }

    private IEnumerator EndProgressBar(float duration)
    {
        yield return new WaitForSeconds(duration);
        isActive = false;
        progressBar.SetActive(false);
        MicrophoneIcon.SetActive(false);
        activeCoroutine = null;
    }
}
