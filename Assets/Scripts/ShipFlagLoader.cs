using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Used to display the correct flag and recall it from an earlier set state.
public class ShipFlagLoader : MonoBehaviour
{
    
    int ChosenFlag = 0;
    void OnEnable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        ChosenFlag = PlayerPrefs.GetInt("ChosenFlag");
        if (ChosenFlag != 0)
        {
            GameObject flag = transform.GetChild(ChosenFlag - 1).gameObject;
            flag.SetActive(true);
        }
        else
        {
            //If no flag was set, just set the first one.
            GameObject firstFlag = transform.GetChild(0).gameObject;
            firstFlag.SetActive(true);
        }
    }


}
