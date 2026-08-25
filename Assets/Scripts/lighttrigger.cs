/*
 * lighttrigger - Activates light and sound effect on collision
 *
 * This script handles trigger-based activation:
 * - Plays sound effect when object enters trigger zone
 * - Can be used to activate lights or other effects
 * - Simple OnTriggerEnter callback implementation
 */

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class lighttrigger : MonoBehaviour
{

    public AudioSource soundEffect;
    public float delay = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (soundEffect != null)
        {
            soundEffect.Play();
        }
    }
   
}


