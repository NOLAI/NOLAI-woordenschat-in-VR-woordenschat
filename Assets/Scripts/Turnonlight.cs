/*
 * Turnonlight - Activates light and plays sound on collision
 *
 * This script handles trigger-based activation:
 * - Enables assigned light when object enters trigger
 * - Plays sound effect on activation
 * - Requires object tag check for specific triggers
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turnonlight : MonoBehaviour
{
    public Light lightToEnable;
    public AudioSource soundEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "object")
        {
            if (lightToEnable != null)
            {
                lightToEnable.enabled = true;
            }
            else
            {
                Debug.LogWarning("No light assigned to lightToEnable in the Inspector.");
            }

            if (soundEffect != null)
            {
                soundEffect.Play();
            }
        }
    }
}
