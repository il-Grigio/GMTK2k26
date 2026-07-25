using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using Grigios;
public class CameraShake : Singleton<CameraShake>
{

    [SerializeField] CinemachineBasicMultiChannelPerlin noise;
    private Coroutine currentShake;

    public void StartShake(float amplitude, float frequency, float duration)
    {
        if (currentShake != null)
            StopCoroutine(currentShake);

        currentShake = StartCoroutine(ShakeRoutine(amplitude, frequency, duration));
    }

    private IEnumerator ShakeRoutine(float amplitude, float frequency, float duration)
    {
        noise.FrequencyGain = frequency;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // decresce da 'amplitude' a 0 in modo lineare
            noise.AmplitudeGain = Mathf.Lerp(amplitude, 0f, elapsed / duration);
            yield return null;
        }

        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
        currentShake = null;
    }
}