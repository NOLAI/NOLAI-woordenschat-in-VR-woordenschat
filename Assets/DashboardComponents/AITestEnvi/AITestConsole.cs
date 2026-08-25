using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AITestConsole : MonoBehaviour
{
    public TextMeshProUGUI g_textConsole;
    public TextMeshProUGUI g_textResponse;
    public MicrophoneClip microphoneClip;

    private Coroutine timerCoroutine;
    private float timerSeconds;

    public void VraagAnker()
    {
        StartAudioClip("A2", "M1");
    }

    public void VraagSchip()
    {
        StartAudioClip("A2", "M2");
    }

    public void VraagVuurtoren()
    {
        StartAudioClip("A3", "M5");
    }

    void StartAudioClip(string activity, string moment)
    {
        microphoneClip.StreamShortRecording(activity, moment);
        StartCoroutine(ConsoleHandler());
    }

    IEnumerator ConsoleHandler()
    {
        float duration = microphoneClip.duration;
        g_textConsole.text = $"Microfoon recording gestart, beantwoord de vraag binnen {duration} sec.";
        yield return new WaitForSeconds(duration);

        g_textConsole.text = "Antwoord opgenomen en verzonden...";
        timerCoroutine = StartCoroutine(StartTimer());
    }

    IEnumerator StartTimer()
    {
        timerSeconds = 0f;

        while (true)
        {
            timerSeconds += 1f;
            g_textConsole.text = $"Antwoord opgenomen en verzonden... {timerSeconds} sec";
            yield return new WaitForSeconds(1f);
        }
    }

    public void DisplayResponse(string response)
    {
        g_textResponse.text = $"Response: {response}";

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            g_textConsole.text = $"Antwoord van de AI is binnen in {timerSeconds} sec!";
        }
    }
}
