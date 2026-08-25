using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

/* This script will hold a list of waypoints on which we can draw a route for the boat to follow */
public class BoatGuideManager : MonoBehaviour
{
    private Dictionary<int, int> currentWaypointIndex = new Dictionary<int, int>();
    public List<List<Transform>> routes = new List<List<Transform>>();

    private Transform previousWaypoint = null;

    public GameObject finalDestination;
    private int lastPoint = 0;

    void Start()
    {
        LoadRoutesFromHierarchy();
    }

    void LoadRoutesFromHierarchy()
    {
        routes.Clear();
        foreach (Transform route in transform) // Loop through child objects (routes)
        {
            List<Transform> waypoints = new List<Transform>();

            foreach (Transform waypoint in route) // Loop through waypoints inside route
            {
                waypoints.Add(waypoint);
            }

            if (waypoints.Count > 1) // Only add if there are valid waypoints
            {
                routes.Add(waypoints);
                Debug.Log("Route loaded with " + waypoints.Count + " waypoints");
            }
        }
    }

    public void ProgressToNextPoint(){
        foreach (List<Transform> route in routes) // Loop through all routes
        {
            if (!currentWaypointIndex.ContainsKey(route.GetHashCode()))
            {
            currentWaypointIndex.Add(route.GetHashCode(), 0);
            }

            int currentIndex = currentWaypointIndex[route.GetHashCode()];
            if (route.Count > 0)
            {
            route.RemoveAt(currentIndex); // Remove the current waypoint
            if (route.Count > 0)
            {
                currentWaypointIndex[route.GetHashCode()] = currentIndex % route.Count; // Update index to stay within bounds
            }
            }
        }
    }

    public Vector3 GetClosestPoint(Vector3 currentPosition)
    {
        // Vector3 closestPoint = Vector3.zero; // Default to (0,0,0) in case no points are found
        Vector3 closestPoint = finalDestination.transform.position;
        float minDistance = Mathf.Infinity;

        foreach (List<Transform> route in routes) // Loop through all routes
        {
            for (int i = 0; i < route.Count; i++) // Loop through all waypoints in the route
            {
                try  {closestPoint = route[lastPoint+1].position;}
                catch (Exception) { closestPoint = finalDestination.transform.position;}
                float distance = Vector3.Distance(currentPosition, route[i].position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = route[i].position;
                    lastPoint = i;
                }
            }
        }

        Debug.DrawLine(currentPosition, closestPoint, Color.red);
        return closestPoint;
    }
}