/*
 * SetObjectToLocation - Moves an object to a target location
 *
 * This script transfers position and rotation from a target
 * location to another object when SetLocation() is called.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetObjectToLocation : MonoBehaviour
{

    public GameObject objectToSet; // The object to set the position of
    public GameObject targetLocation; // The target location to set the object to
    // Start is called before the first frame update
    public void SetLocation() {
        if (objectToSet != null && targetLocation != null)
        {
            objectToSet.transform.position = targetLocation.transform.position;
            objectToSet.transform.rotation = targetLocation.transform.rotation;
        }
        else
        {
            Debug.LogWarning("Object to set or target location is not assigned.");
        }
    }
}
