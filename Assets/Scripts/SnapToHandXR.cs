/*
 * SnapToHandXR - Snaps grabbed object to player's hand
 *
 * This script handles VR hand snapping:
 * - Smoothly moves object to hand when grabbed
 * - Releases object when hand lets go
 * - Configurable snap speed and threshold
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SnapToHandXR : MonoBehaviour
{
    // Speed of snapping the object to the hand
    public float snapSpeed = 5f;

    // The distance threshold to consider when the object should snap
    public float snapThreshold = 0.1f;

    // Reference to the XR Grab Interactable component
    private XRGrabInteractable grabInteractable;

    // Reference to the object being snapped
    private bool isSnapping = false;

    private Transform handTransform;
    private Transform originalParent;

    private void Awake()
    {
        // Get the XR Grab Interactable component from the object
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to the select (grab) and deselect (release) events
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    // Called when the object is grabbed
    private void OnGrab(SelectEnterEventArgs args)
    {
        // Get the interactor's hand transform (controller transform)
        handTransform = args.interactorObject.transform;
        originalParent = transform.parent;

        // Start snapping the object to the hand
        isSnapping = true;
    }

    // Called when the object is released
    private void OnRelease(SelectExitEventArgs args)
    {
        // Stop snapping and clear references
        isSnapping = false;
        handTransform = null;

        // Optionally restore original parent
        transform.SetParent(originalParent);
    }

    private void Update()
    {
        if (isSnapping && handTransform != null)
        {
            // Smoothly move the object to the hand's position
            transform.position = Vector3.Lerp(transform.position, handTransform.position, snapSpeed * Time.deltaTime);

            // Smoothly rotate the object to align with the hand's rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, handTransform.rotation, snapSpeed * Time.deltaTime);

            // Parent the object to the hand once it reaches close enough
            if (Vector3.Distance(transform.position, handTransform.position) < snapThreshold)
            {
                transform.SetParent(handTransform);
                isSnapping = false; // Snap is complete
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }
}
