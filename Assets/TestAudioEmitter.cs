using System;
using UnityEngine;

public class TestAudioEmitter : MonoBehaviour
{
    private void Start()
    {
        AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.swordSFX, transform.position);
    }
}
