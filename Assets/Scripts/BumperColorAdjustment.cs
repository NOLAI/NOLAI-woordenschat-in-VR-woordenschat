using UnityEngine;
using System;


// This script is used to color the bumper buttons of the controller to a different color when the tutorial requires it
public class ColorSwitcher : MonoBehaviour
{
    public Material targetMaterial; // The material to change colors
    public Color color1 = Color.red; // First color
    public Color color2 = Color.blue; // Second color
    public Color defaultColor = Color.white; // Default color

    public float switchInterval = 1f; // Time interval for switching colors

    private float timer;
    private bool isColor1 = true;
    private bool isSwitching = false;

    // Events to start and stop color switching
    public static event Action OnStartSwitching;
    public static event Action OnStopSwitching;

    void Start()
    {
        if (targetMaterial == null)
        {
            Debug.LogError("Target material not assigned. Please assign a material in the inspector.");
        }
        else
        {
            targetMaterial.color = defaultColor;
        }

        // Subscribe to events
        OnStartSwitching += StartSwitching;
        OnStopSwitching += StopSwitching;
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        OnStartSwitching -= StartSwitching;
        OnStopSwitching -= StopSwitching;
    }

    void Update()
    {
        if (!isSwitching || targetMaterial == null)
            return;

        timer += Time.deltaTime;

        if (timer >= switchInterval)
        {
            timer = 0f;
            isColor1 = !isColor1;
            targetMaterial.color = isColor1 ? color1 : color2;
        }
    }

    private void StartSwitching()
    {
        isSwitching = true;
        timer = 0f; // Reset timer
        Debug.Log("Starting to highlight button color");
    }

    private void StopSwitching()
    {
        isSwitching = false;
        targetMaterial.color = defaultColor;
    }

    // Optional public methods to trigger events
    public static void TriggerStartSwitching()
    {
        OnStartSwitching?.Invoke();
    }

    public static void TriggerStopSwitching()
    {
        OnStopSwitching?.Invoke();
    }
}
