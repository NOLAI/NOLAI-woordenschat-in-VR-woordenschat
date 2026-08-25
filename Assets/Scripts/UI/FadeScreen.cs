using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class FadeScreen : MonoBehaviour
{
    public bool fadeOnStart = true;
    public float fadeDuration = 22;
    public Color fadeColor;

    [Header("Actions")]
    public UnityEvent OnFadeInStart;
    public UnityEvent OnFadeOutStart;

    [Header("Optional")]
    public bool onlyFadeOut = true;
    public Color defaultImageColor;
    public Color defaultTextColor;
    public Image[] loadingScreenImages;
    public TextMeshProUGUI[] loadingScreenText;
    public Slider loadingbarSlider;

    private Renderer rend;


    private void Start()
    {
        rend = GetComponent<Renderer>();

        if (fadeOnStart)
            FadeIn();

    }

    public void FadeIn()
    {
        if (onlyFadeOut)
        {
            foreach (Image image in loadingScreenImages)
                image.enabled = false;

            foreach (TextMeshProUGUI text in loadingScreenText)
                text.enabled = false;
        }

        Fade(1, 0);
    }

    public void FadeOut()
    {
        if (onlyFadeOut)
        {
            foreach (Image image in loadingScreenImages)
                image.enabled = true;

            foreach (TextMeshProUGUI text in loadingScreenText)
                text.enabled = true;
        }

        Fade(0, 1);
    }

    private void Fade(float alphaIn, float alphaOut)
    {
        StartCoroutine(FadeRoutine(alphaIn, alphaOut));
    }

    private IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {
        if (alphaIn == 1)
            OnFadeInStart.Invoke();
        else if (alphaIn == 0)
            OnFadeOutStart.Invoke();

        float timer = 0;
        while (timer <= fadeDuration)
        {
            Color newColor = fadeColor;
            Color newImageColor = defaultImageColor;
            Color newTextColor = defaultTextColor;

            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);
            newImageColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);
            newTextColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);

            rend.material.SetColor("_BaseColor", newColor);

            if (loadingScreenImages.Length > 0)
            {
                foreach (Image image in loadingScreenImages)
                    image.color = newImageColor;
            }

            if (loadingScreenText.Length > 0)
            {
                foreach (TextMeshProUGUI text in loadingScreenText)
                    text.color = newTextColor;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Color finalColor = fadeColor;
        Color finalImageColor = defaultImageColor;
        Color finalTextColor = defaultTextColor;

        finalColor.a = alphaOut;
        finalImageColor.a = alphaOut;
        finalTextColor.a = alphaOut;

        rend.material.SetColor("_BaseColor", finalColor);

        if (loadingScreenImages.Length > 0)
        {
            foreach (Image image in loadingScreenImages)
                image.color = finalImageColor;
        }

        if (loadingScreenText.Length > 0)
        {
            foreach (TextMeshProUGUI text in loadingScreenText)
                text.color = finalTextColor;
        }

    }
}
