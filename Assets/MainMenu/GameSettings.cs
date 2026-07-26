using System;
using Grigios;
using UnityEngine;
using UnityEngine.Audio;

public class GameSettings : Singleton<GameSettings>
{
    [Header("Audio Mixer")]
    [Tooltip("AudioMixer con parametri esposti MusicVolume e SFXVolume (vedi SettingsUIController per il setup)")]
    public AudioMixer audioMixer;

    // ---------------- Stato corrente (in memoria) ----------------
    public float MusicVolume { get; private set; } = 0.70f;
    public float SfxVolume { get; private set; } = 0.70f;
    public float AmbianceVolume { get; private set; } = 0.70f;
    
    public event Action OnSettingsChanged;

    private const string PREFIX = "gs_";
    private bool audioUnlocked = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        ApplyAll();

#if UNITY_WEBGL && !UNITY_EDITOR
        // I browser bloccano l'audio finche' non c'e' un'interazione dell'utente.
        // Mettiamo in pausa l'AudioListener finche' non arriva il primo input.
        AudioListener.pause = true;
        audioUnlocked = false;
#else
        audioUnlocked = true;
#endif
    }

    // ==================== AUDIO ====================

    public void SetMusicVolume(int linear01)
    {
        MusicVolume = Mathf.Clamp01(linear01 * 0.1f);
        ApplyMixerVolume("MusicVolume", MusicVolume);
        PlayerPrefs.SetFloat(PREFIX + "music_vol", MusicVolume);
        Notify();
    }

    public void SetSfxVolume(int linear01)
    {
        SfxVolume = Mathf.Clamp01(linear01 * 0.1f);
        ApplyMixerVolume("SFXVolume", SfxVolume);
        PlayerPrefs.SetFloat(PREFIX + "sfx_vol", SfxVolume);
        Notify();
    }

    public void SetAmbianceVolume(int linear01)
    {
        AmbianceVolume = Mathf.Clamp01(linear01 * 0.1f);
        ApplyMixerVolume("AmbianceVolume", AmbianceVolume);
        PlayerPrefs.SetFloat(PREFIX + "ambiance_vol", AmbianceVolume);
        Notify();
    }

    private void ApplyMixerVolume(string exposedParam, float linearVolume)
    {
        if (audioMixer == null) return;
        float clamped = Mathf.Clamp(linearVolume, 0.0001f, 1f);
        float dB = linearVolume <= 0.0001f ? -80f : Mathf.Log10(clamped) * 20f;
        audioMixer.SetFloat(exposedParam, dB);
    }
    
    /// <summary>Riporta tutte le impostazioni ai valori di default. Collegalo a un pulsante "Ripristina predefiniti".</summary>
    public void ResetSettingsToDefaults()
    {
        ApplyAll();
        Notify();
    }


    // ==================== LOAD / APPLY ====================

    /// <summary>Applica tutto lo stato corrente al motore. Chiamalo anche dopo un cambio scena se serve.</summary>
    public void ApplyAll()
    {
        ApplyMixerVolume("MusicVolume", MusicVolume);
        ApplyMixerVolume("SFXVolume", SfxVolume);
        ApplyMixerVolume("AmbianceVolume", AmbianceVolume);
    }

    private void Notify() => OnSettingsChanged?.Invoke();
}
