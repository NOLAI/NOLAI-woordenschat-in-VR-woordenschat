/*
 * GrabMovementTranslate - Translates grabbed objects along an axis
 *
 * This script handles vertical movement of grabbed objects:
 * - Moves object between minimum and maximum limits
 * - Supports lowering mode (flag animation)
 * - Triggers event when limit is reached
 * - Can invert direction and apply speed multiplier
 */

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabMovementTranslate : MonoBehaviour
{
    public GameObject target;
    private Vector3 previousPosition;
    private Vector3 originalPosition;
    public bool ResetPositionAfterRelease = false;
    public bool InvertDirection = false;
    public float SpeedMultiplier = 1f;
    public GameObject ObjectToScroll;
    public bool LoweringMode = false;

    public Transform MaximumLimit;
    public Transform MinimumLimit;

    private XRGrabInteractable grabInteractable;
    private Renderer rend;
    private bool hasReachedBoundary = false;

    public UnityEvent<float> OnLimitReached;

    void Start()
    {
        originalPosition = transform.position;
        previousPosition = transform.position;
        rend = ObjectToScroll?.GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEnter);
            grabInteractable.selectExited.AddListener(OnSelectExit);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEnter);
            grabInteractable.selectExited.RemoveListener(OnSelectExit);
        }
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        hasReachedBoundary = false;
    }

    void Update()
    {
        if (!grabInteractable || !grabInteractable.isSelected)
        {
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 deltaPosition = currentPosition - previousPosition;

        if (InvertDirection)
        {
            deltaPosition *= -1;
        }

        float newYPosition = target.transform.position.y + (deltaPosition.y * SpeedMultiplier);
        Vector3 newTargetPosition = target.transform.position;
        newTargetPosition.y = newYPosition;

        if (LoweringMode)
        {
            if (MaximumLimit != null && newYPosition >= MaximumLimit.position.y)
            {
                newTargetPosition.y = MaximumLimit.position.y;
                hasReachedBoundary = true;
            }
            if (MinimumLimit != null && !hasReachedBoundary && newYPosition <= MinimumLimit.position.y)
            {
                newTargetPosition.y = MinimumLimit.position.y;
                OnLimitReached?.Invoke(newTargetPosition.y);
                Debug.Log("Success! Minimum limit reached (lowering).");
            }
        }
        else
        {
            if (MinimumLimit != null && newYPosition <= MinimumLimit.position.y)
            {
                newTargetPosition.y = MinimumLimit.position.y;
                hasReachedBoundary = true;
            }
            if (MaximumLimit != null && !hasReachedBoundary && newYPosition >= MaximumLimit.position.y)
            {
                newTargetPosition.y = MaximumLimit.position.y;
                OnLimitReached?.Invoke(newTargetPosition.y);
                Debug.Log("Success! Maximum limit reached (lifting).");
            }
        }

        if (!hasReachedBoundary)
        {
            target.transform.position = newTargetPosition;
        }

        if (rend != null && !hasReachedBoundary)
        {
            Material material = rend.material;
            Vector2 currentOffset = material.GetTextureOffset("_MainTex");
            Vector2 scrollAmount = new Vector2(0, deltaPosition.y);
            material.SetTextureOffset("_MainTex", currentOffset + scrollAmount);
        }

        previousPosition = currentPosition;
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        if (ResetPositionAfterRelease)
        {
            transform.position = originalPosition;
            previousPosition = originalPosition;
        }
    }
}
