using System;
using System.Collections.Generic;
using FMOD.Studio;
using Grigios;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : Singleton<AudioManager>
{
    private List<EventInstance> eventInstances = new List<EventInstance>();
    private EventInstance musicEventInstance;
    private EventInstance ambienceBackground;
    private void Awake()
    {
        eventInstances = new List<EventInstance>();
    }

    public void Start()
    {
        InitializeMusic(FMODEventsManager.Instance.baseMusic);
        InitializeAmbience(FMODEventsManager.Instance.ambiance);
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(sound, worldPosition);
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    private void CleanUp()
    {
        foreach (var eventInstance in eventInstances)
        {
            eventInstance.stop(STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }

    private void InitializeMusic(EventReference musicEventReference)
    {
        musicEventInstance = CreateInstance(musicEventReference);
        musicEventInstance.start();
    }
    public void SetMusicState(int state)
    {
        // 0 = STEALTH, 1 CAOS, 2 CAOS2, 3 CAOS3, 4 = DUEL
        musicEventInstance.setParameterByName("MusicState", state);
    }

    private void InitializeAmbience(EventReference ambienceEventReference)
    {
        ambienceBackground = CreateInstance(ambienceEventReference);
        ambienceBackground.start();
    }
    private void OnDestroy()
    {
        CleanUp();
    }
    //what to do in playerMovement to play steps:
    /*
    private void Start()
    {
        playerFootsteps = AudioManager.Instance.CreateInstance(FMODEventsManager.Instance.playerFootstepsSFX);
    }
    private void UpdateSound()
    {
        if (isWalking)
        {
            PLAYBACK_STATE playbackState;
            playerFootsteps.getPlaybackState(out playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                playerFootsteps.start();
            }
            else
            {
                playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
    */
}
