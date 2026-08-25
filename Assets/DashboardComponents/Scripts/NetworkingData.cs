using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkingData : MonoBehaviour
{
    public string serverURL;
    public string serverPort;
    string localURL;
    public string GetLocalURL() => localURL; // unit-testing access
    public string localPort = "5000";

    void Start()
    {
        localURL = GetLocalIPAddress();
        Debug.Log("Local IP Address: " + localURL);
        GetComponent<MJPEGStreamer>().SetAdress(localURL, localPort);
        StartCoroutine(SendStreamData(localURL, localPort));
    }

    //gaf voorheen het IP adres waar het spel op draait, maar alss het op een VR set draait werkt dat niet
    // geeft nu altijd het IP adres van de laptop
    string GetLocalIPAddress()
    {
        //string ipAddress = "127.0.0.1"; // Fallback

        //foreach (var networkInterface in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        //{
        //    if (networkInterface.AddressFamily == AddressFamily.InterNetwork)
        //    {
        //        ipAddress = networkInterface.ToString();
        //        break;
        //    }
        //}
        //return ipAddress;

        return "192.168.1.250"; //IP adres van de victus waar de docker services draaien
    }

    public IEnumerator SendStreamData(string ip, string port)
    {
        WWWForm form = new WWWForm();
        form.AddField("ip", ip);
        form.AddField("port", port);

        string laravelAPI = $"http://{serverURL}:{serverPort}/update-stream";
        
        using (UnityWebRequest www = UnityWebRequest.Post(laravelAPI, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Fout bij updaten van Laravel: " + www.error);
            }
            else
            {
                Debug.Log("Stream info succesvol naar Laravel gestuurd.");
            }
        }
    }
}