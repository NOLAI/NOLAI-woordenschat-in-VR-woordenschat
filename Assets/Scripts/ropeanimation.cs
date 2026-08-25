/*
 * ropeanimation - Triggers rope animation when grabbed
 *
 * This script plays an animation on grab:
 * - Detects when object is grabbed via XRGrabInteractable
 * - Triggers the specified animation (e.g., "rope" trigger)
 * - Uses Animator component for animation playback
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ropeanimation : MonoBehaviour
{
    public string animationTrigger = "rope"; // The trigger parameter name in the Animator
    private XRGrabInteractable grabInteractable;
    private Animator animator;

    private void Start()
    {
        // Get the XRGrabInteractable and Animator components
        grabInteractable = GetComponent<XRGrabInteractable>();
        animator = GetComponent<Animator>();

        // Subscribe to the grab event
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    // Called when the object is grabbed
    private void OnGrab(SelectEnterEventArgs args)
    {
        // Set the trigger to play the animation
        animator.SetTrigger(animationTrigger);
    }
}
