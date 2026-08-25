using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationProcessCanvas : MonoBehaviour
{
    [SerializeField] GameObject listeningIconObject;
    [SerializeField] GameObject thinkingIconObject;
    [SerializeField] GameObject speakingIconObject;

    public GameObject ListeningIconObject { get => listeningIconObject; }
    public GameObject ThinkingIconObject { get => thinkingIconObject; }
    public GameObject SpeakingIconObject1 { get => speakingIconObject; }

    public void SetVisibilityCanvas(bool enabled)
    {
        listeningIconObject.SetActive(enabled);
        thinkingIconObject.SetActive(enabled);
        speakingIconObject.SetActive(enabled);
    }
}
