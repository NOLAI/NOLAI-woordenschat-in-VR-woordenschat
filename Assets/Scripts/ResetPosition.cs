/*
 * ResetPosition - Resets object to its initial position and rotation
 *
 * This script stores the initial transform state and provides
 * a method to reset the object back to those saved values.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reset()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}
