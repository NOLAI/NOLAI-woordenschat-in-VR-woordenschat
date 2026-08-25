/*
 * GrabRotatePlank - Rotates grabbed objects between angle limits
 *
 * This script handles rotational movement of grabbed objects:
 * - Rotates object between start and end angles
 * - Triggers event when angle limit is reached
 * - Works for movable planks and similar objects
 * - Supports texture scrolling on the object
 * - Used for putting the walking plank onto the boat
 */

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using static GrabMovementTranslate;

public class GrabRotatePlank : MonoBehaviour
{
    public GameObject target;
    public float StartAngle = 0f;
    public float EndAngle = 90f;
    public float SpeedMultiplier = 1f;
    public GameObject ObjectToScroll;

    private XRGrabInteractable grabInteractable;
    private Vector3 previousPosition;
    private Vector3 originalPosition;
    private Renderer rend;
    private bool hasReachedLimit = false;

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
        hasReachedLimit = false;
    }

    void Update()
    {
        if (!grabInteractable || !grabInteractable.isSelected)
        {
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 deltaPosition = currentPosition - previousPosition;

        float rotationChange = deltaPosition.y * SpeedMultiplier;
        float currentAngle = target.transform.localEulerAngles.x;
        currentAngle = NormalizeAngle(currentAngle);
        float newAngle = currentAngle - rotationChange;

        if (newAngle > StartAngle)
        {
            newAngle = StartAngle;
            hasReachedLimit = true;
            Debug.Log("Hard limit reached at " + newAngle);
        }
        else if (newAngle < EndAngle)
        {
            newAngle = EndAngle;
            hasReachedLimit = true;
            OnLimitReached?.Invoke(newAngle);
            Debug.Log("Success! Maximum limit reached (lifting).");
        }

        if (!hasReachedLimit)
        {
            target.transform.localEulerAngles = new Vector3(newAngle, target.transform.localEulerAngles.y, target.transform.localEulerAngles.z);
        }

        if (rend != null && !hasReachedLimit)
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
        transform.position = originalPosition;
        previousPosition = originalPosition;
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0)
        {
            angle += 360f;
        }
        return angle;
    }
}