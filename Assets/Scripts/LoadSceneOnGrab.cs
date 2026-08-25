/*
 * LoadSceneOnGrab - Loads a new scene when object is grabbed
 *
 * This script handles scene transitions:
 * - Loads a specified scene when object is grabbed
 * - Uses XRGrabInteractable for VR grabbing detection
 * - Optional scene name assignment in Inspector
 * - Includes cleanup to prevent memory leaks
 * - Gets used in the MENU scene to switch between assignments
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class LoadSceneOnGrab : MonoBehaviour
{
    [Tooltip("Scene name to load")]
    public string sceneToLoad;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
        else
        {
            Debug.LogError("XRGrabInteractable not found! Please ensure this object has an XRGrabInteractable component.");
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("No scene assigned in the Inspector!");
        }
    }
}
