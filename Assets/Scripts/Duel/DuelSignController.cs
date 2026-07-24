
using System;
using UnityEngine;

public class DuelSignController : MonoBehaviour
{
    [SerializeField] private DuelSign[] signs;
    private void OnEnable()
    {
        DuelManager.Instance.OnCountSign -= OnCountdown;
        DuelManager.Instance.OnCountSign += OnCountdown;
        DuelManager.Instance.OnStepForward -= OnInterrupt;
        DuelManager.Instance.OnStepForward += OnInterrupt;
        foreach (var sign in signs)
        {
            sign.gameObject.SetActive(false);
        }
    }
    private void OnDisable()
    {
        DuelManager.Instance.OnStepForward -= OnInterrupt;
        DuelManager.Instance.OnCountSign -= OnCountdown;
    }
    private void OnCountdown(int i)
    {
        signs[signs.Length - 1 - i].gameObject.SetActive(true);
    }
    public void OnInterrupt()
    {
        foreach (var sign in signs)
        {
            if (sign.gameObject.activeSelf)
                sign.Interrupt();
        }
    }
}
