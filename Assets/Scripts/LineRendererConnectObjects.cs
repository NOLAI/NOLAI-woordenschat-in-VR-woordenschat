using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is used together with a line renderer to connect two game objects with one another by drawing a line in between.
public class LineRendererConnectObjects : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;

    private LineRenderer lineRenderer;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Set line properties
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    void Update()
    {
        if (pointA != null && pointB != null)
        {
            // Update line positions
            lineRenderer.SetPosition(0, pointA.transform.position);
            lineRenderer.SetPosition(1, pointB.transform.position);
        }
    }
}
