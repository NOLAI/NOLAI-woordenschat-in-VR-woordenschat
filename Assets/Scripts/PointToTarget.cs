using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is used to rotate the game object to point towards the specified transform point
public class PointToTarget : MonoBehaviour
{
    public bool PointX = false;
    public bool PointY = true;
    public bool PointZ = false;
    public Transform target;
    public bool flipRotation = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        Vector3 eulerAngles = rotation.eulerAngles;

        if (!PointX)
        {
            eulerAngles.x = 0f;
        }
        if (!PointY)
        {
            eulerAngles.y = 0f;
        }
        if (!PointZ)
        {
            eulerAngles.z = 0f;
        }

        if (flipRotation)
        {
            eulerAngles.y += 180f;
        }

        transform.rotation = Quaternion.Euler(eulerAngles);
    }
}
