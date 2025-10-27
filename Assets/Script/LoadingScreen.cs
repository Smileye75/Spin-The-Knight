using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Image fadeImage;        // assign in Inspector (UI > Image)
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool blockRaycastsWhileVisible = true;

    public IEnumerator FadeIn(float duration)
    {
        if (!fadeImage) yield break;

        // make sure it's visible & on top
        if (!fadeImage.gameObject.activeSelf) fadeImage.gameObject.SetActive(true);

        Color c = fadeImage.color;
        float start = c.a;
        float end = 1f;
        float t = 0f;

        if (blockRaycastsWhileVisible) fadeImage.raycastTarget = true;

        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            c.a = Mathf.Lerp(start, end, duration > 0f ? t / duration : 1f);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;
    }

    public IEnumerator FadeOut(float duration)
    {
        if (!fadeImage) yield break;

        Color c = fadeImage.color;
        float start = c.a;
        float end = 0f;
        float t = 0f;

        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            c.a = Mathf.Lerp(start, end, duration > 0f ? t / duration : 1f);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;

        if (blockRaycastsWhileVisible) fadeImage.raycastTarget = false;
    }
}
