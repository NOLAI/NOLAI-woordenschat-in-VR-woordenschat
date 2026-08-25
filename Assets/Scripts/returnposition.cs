/*
 * returnposition - Automatically returns object if moved too far
 *
 * This script monitors the object's distance from its original position
 * and automatically snaps it back if it exceeds the distance threshold.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class returnposition : MonoBehaviour
{
    Vector3 originalPos;
    public float distanceTreshold = 5f;
    float distance;

    void Start()
    {
        originalPos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z); 
    }

    void Update()
    {
        distance = Vector3.Distance(gameObject.transform.position, originalPos);
        //Debug.Log(distance);
        if (distance > distanceTreshold)
        {
            Debug.Log("Object te ver weg");
            transform.position = originalPos;
        }
    }

}
