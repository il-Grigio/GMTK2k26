using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class DuelSign : MonoBehaviour
{
    [SerializeField] float growDuration = 0.4f;
    [SerializeField] float visibleTime = 1.5f;
    [SerializeField] float hideDuration = 0.4f;
    [SerializeField] AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 scalaOriginal;

    private Coroutine coroutineCurrent;
    void Awake()
    {
        scalaOriginal = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void OnEnable()
    {
        StartCoroutine(AnimationPopup());
    }

    IEnumerator AnimationPopup()
    {
        transform.localScale = Vector3.zero;
        yield return StartCoroutine(Scale(Vector3.zero, scalaOriginal, growDuration));
        yield return new WaitForSeconds(visibleTime);
        yield return StartCoroutine(Scale(scalaOriginal, Vector3.zero, hideDuration));
        gameObject.SetActive(false);
    }

    public void Interrupt()
    {
        if (coroutineCurrent != null)
        {
            StopCoroutine(coroutineCurrent);
        }

        coroutineCurrent = StartCoroutine(HideNow());
    }
    IEnumerator HideNow()
    {
        yield return StartCoroutine(Scale(transform.localScale, Vector3.zero, hideDuration));
        gameObject.SetActive(false);
    }
    IEnumerator Scale(Vector3 da, Vector3 a, float durata)
    {
        float t = 0f;
        while (t < durata)
        {
            t += Time.deltaTime;
            float progresso = curve.Evaluate(t / durata);
            transform.localScale = Vector3.LerpUnclamped(da, a, progresso);
            yield return null;
        }
        transform.localScale = a;
    }
}