/*
 * Lerp.cs - Smoothly lerps grabbed object to a target position
 *
 * This script handles smooth movement of grabbed objects:
 * - Uses Linear Interpolation (Lerp) for smooth motion
 * - Moves object to a specified target when grabbed
 * - Configurable lerp duration for timing control
 * - Coroutine-based for frame-rate independent movement
 */

using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LerpToTarget : MonoBehaviour
{
    public GameObject targetObject;  // The game object to move the grabbed object towards
    public float lerpDuration = 1.5f;  // Time it takes to move to the target

    private XRGrabInteractable grabInteractable;
    private bool isLerping = false;

    private void Start()
    {
        // Get the XRGrabInteractable component attached to this GameObject
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to the 'onSelectEntered' event, which triggers when the object is grabbed
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
        } else
        {
            Debug.Log("No grab interactable found! Please add a XR Grab interactable to " + this.gameObject.name);
        }
    }

    // This function is called when the object is grabbed
    private void OnGrab(SelectEnterEventArgs args)
    {
        // Start the Lerp coroutine to move towards the target object
        if (!isLerping && targetObject != null)
        {
            StartCoroutine(LerpPosition(targetObject.transform.position, lerpDuration));
        }
    }

    // Coroutine to smoothly move the object towards the target position
    private IEnumerator LerpPosition(Vector3 targetPosition, float duration)
    {
        isLerping = true;
        float time = 0;
        Vector3 startPosition = transform.position;

        // Lerp the object to the target position
        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        // Ensure the object is exactly at the target position
        transform.position = targetPosition;

        isLerping = false;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        if (grabInteractable != null) { 
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }
}
