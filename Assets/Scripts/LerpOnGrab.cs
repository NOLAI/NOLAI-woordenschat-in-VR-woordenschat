/*
 * LerpOnGrab - Lerps grabbed object to the grabbing hand
 *
 * This script handles automatic lerp movement:
 * - Moves object smoothly to the player's hand when grabbed
 * - Uses Linear Interpolation for smooth animation
 * - Configurable duration for the lerp effect
 * - Coroutine-based for smooth frame-by-frame movement
 */

using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LerpOnGrab : MonoBehaviour
{
    public float lerpDuration = 1.5f;    // Time it takes to move to the hand
    private XRGrabInteractable grabInteractable;
    private bool isLerping = false;

    private void Start()
    {
        // Get the XRGrabInteractable component attached to this GameObject
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to the 'onSelectEntered' event, which triggers when the object is grabbed
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    // This function is called when the object is grabbed
    private void OnGrab(SelectEnterEventArgs args)
    {
        // Get the interactor (the hand/controller that grabbed the object)
        IXRSelectInteractor grabbingInteractor = args.interactorObject;

        // Start the Lerp coroutine to move towards the grabbing object (hand/controller)
        if (!isLerping)
        {
            StartCoroutine(LerpPosition(grabbingInteractor, lerpDuration));
        }
    }

    // Coroutine to smoothly move the object towards the grabbing hand
    private IEnumerator LerpPosition(IXRSelectInteractor grabbingInteractor, float duration)
    {
        isLerping = true;
        float time = 0;
        Vector3 startPosition = transform.position;
        Transform grabbingTransform = grabbingInteractor.transform;

        // Lerp the object to the grabbing hand's position
        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, grabbingTransform.position, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        // Ensure the object is exactly at the grabbing hand's position
        transform.position = grabbingTransform.position;

        // Now we manually force the object to attach to the grabbing hand using the XR Interaction Manager
        grabInteractable.interactionManager.SelectEnter(grabbingInteractor, grabInteractable);

        // Stop lerping after it's attached to the hand
        isLerping = false;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        grabInteractable.selectEntered.RemoveListener(OnGrab);
    }
}
