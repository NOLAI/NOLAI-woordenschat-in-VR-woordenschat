/*
 * Destroyobject - Destroys game object when bucket collision occurs
 *
 * This script handles object cleanup:
 * - Detects when a "bucket" tag enters the trigger zone
 * - Destroys the game object (or could disable instead)
 * - Prevents issues with ongoing task references
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Destroyobject : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "bucket")
        {
            Destroy(this.gameObject);

            //lets avoid destroying the object and simply disable it to avoid problems with tasks still calling this object.
            //gameObject.SetActive(false);
        }

    }
}
