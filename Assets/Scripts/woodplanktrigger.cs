/*
 * woodplanktrigger - Triggers plank animation on grab
 *
 * This script handles wood plank interactions:
 * - Plays "Grab" animation trigger when plank is grabbed
 * - Uses XRGrabInteractable for VR grabbing detection
 * - Requires Animator component on target object
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class woodplanktrigger : MonoBehaviour
{
    public Animator targetAnimator;
    public string animationTrigger = "Grab"; // The trigger parameter name in the Animator
    private XRGrabInteractable grabInteractable;


    private void Start()
    {
        // Get the XRGrabInteractable and Animator components
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to the grab event
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    // Called when the object is grabbed
    private void OnGrab(SelectEnterEventArgs args)
    {
            targetAnimator.SetTrigger(animationTrigger);

    }
}
