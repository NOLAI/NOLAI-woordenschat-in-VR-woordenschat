using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmDataUpload : MonoBehaviour
{
    PostData postData;
    int confirmations = 0;

    void Start()
    {
        postData = GetComponent<PostData>();
    }

    void Response()
    {
        postData.SendData("Data opgeslagen", "/set-unity-status");
    }

    public void ConfirmUpload()
    {
        confirmations = confirmations+1;
        Debug.Log($"Upload confirmed, total confirms: {confirmations}");

        if (confirmations >= 3)
        {
            ResetConfirm();
            Response();
        }
    }

    public void ResetConfirm()
    {
        confirmations = 0;
    }
}
