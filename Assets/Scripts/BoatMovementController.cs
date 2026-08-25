using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// This script controls a boat using the helm's rotation to steer and keyboard input to control speed.
public class BoatMovementController : MonoBehaviour
{
    public GameObject Helm; // The steering wheel object
    public bool AllowMovement = false;
    public float speed = 0f;
    public float rotationSpeed = 2.0f;
    public float maxSpeed = 10.0f;
    private Quaternion WaypointRotationAdjustment;
    public float maxDistanceBeforeCorrection = 5.0f; // Maximum distance before correction is applied
    public float translationSpeed = 2.0f; // Speed at which the boat corrects its position

    public BoatGuideManager splineManager;
    private float StartHelmRotation;

    void Start()
    {
        StartHelmRotation = Helm.transform.localRotation.eulerAngles.z;
    }

    public void IncreaseSpeed()
    {
        if (speed < maxSpeed)
        {
            speed += 1.0f;
        }
    }

    public void DecreaseSpeed()
    {
        if (speed > 0)
        {
            speed -= 1.0f;
        }
    }

    public void StopShip()
    {
        StartCoroutine(SlowDownToStop());
    }


    private IEnumerator SlowDownToStop()
    {
        float initialSpeed = speed;
        float duration = 3.0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            speed = Mathf.Lerp(initialSpeed, 0, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        speed = 0;
    }


    void Update()
{
    if (AllowMovement)
    {
        float CurrentHelmRotation = Helm.transform.localRotation.eulerAngles.z;

        // Keyboard input for speed
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed)
                speed += 0.05f;
            if (keyboard.sKey.isPressed)
                speed -= 0.1f;
        }
        speed = Mathf.Clamp(speed, 0, maxSpeed);

        Vector3 closestPoint = splineManager.GetClosestPoint(transform.position);
        if (Vector3.Dot(transform.forward, closestPoint - transform.position) < 0)
        {
            splineManager.ProgressToNextPoint();
            closestPoint = splineManager.GetClosestPoint(transform.position);
        }

        float distance = Vector3.Distance(transform.position, closestPoint);

        // Helm rotation
        float rotationDifference = Mathf.DeltaAngle(StartHelmRotation, CurrentHelmRotation);

        // Direction to waypoint
        Vector3 toWaypoint = closestPoint - transform.position;
        float angleToWaypoint = Vector3.SignedAngle(transform.forward, toWaypoint, Vector3.up);

        // HELM INPUT
        float helmSteer = rotationDifference / 80f; //Helm sensitivity (lower is slower turn rate)

        // AUTO-STEERING
        float autoSteer = Mathf.Clamp(angleToWaypoint / 45f, -1f, 1f);  // Auto-steering sensitivity (lower is harder pull)

        // BLEND between helm and auto-steering
        float alignment = Mathf.Clamp01(1f - Mathf.Abs(angleToWaypoint) / 60f); //How far off course you can still use the helm. Higher is more helm freedom

        float finalSteer = Mathf.Lerp(autoSteer, helmSteer, alignment);
        float steerAmount = finalSteer * rotationSpeed * Time.deltaTime;

        // --- DEBUG DRAWING ---
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.blue); // Boat's forward
        Debug.DrawRay(transform.position, Quaternion.Euler(0, angleToWaypoint, 0) * transform.forward * 3f, Color.green); // Direction the boat *needs* to turn
        Debug.DrawLine(closestPoint + Vector3.up * 0.5f, closestPoint + Vector3.up * 2.0f, Color.yellow); // Marker at the waypoint
        // ---

        // Move and rotate
        transform.SetPositionAndRotation(
            transform.position + transform.forward * speed * Time.deltaTime,
            Quaternion.Euler(0f, transform.rotation.eulerAngles.y + steerAmount, 0f)
        );
    }
}
}
