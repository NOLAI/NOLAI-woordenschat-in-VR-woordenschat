using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

//Used to set the favorite flag of the player so it can be recalled later using ShipFlagLoader.cs

public class FlagSelector : MonoBehaviour
{   
    public int FlagIdentifier;

    void OnEnable()
    {
        var grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
    }

    void OnDisable()
    {
        var grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        PlayerPrefs.SetInt("ChosenFlag", FlagIdentifier);
        Debug.Log("Set flag to " + FlagIdentifier);
    }

    public void SetFlag()
    {
        PlayerPrefs.SetInt("ChosenFlag", FlagIdentifier);
    }
}
