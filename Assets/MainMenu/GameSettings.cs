using System;
using FMODUnity;
using FMOD.Studio;
using Grigios;
using UnityEngine;

public class GameSettings : Singleton<GameSettings>
{
    [Header("FMOD VCA Paths")]
    [Tooltip("Path delle VCA create in FMOD Studio (Mixer > VCA)")]
    [SerializeField] private string musicVcaPath = "vca:/MUSIC";
    [SerializeField] private string sfxVcaPath = "vca:/SFX";
    [SerializeField] private string ambianceVcaPath = "vca:/AMBIANCE";

    private VCA musicVCA;
    private VCA sfxVCA;
    private VCA ambianceVCA;

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

        musicVCA = RuntimeManager.GetVCA(musicVcaPath);
        sfxVCA = RuntimeManager.GetVCA(sfxVcaPath);
        ambianceVCA = RuntimeManager.GetVCA(ambianceVcaPath);

        ApplyAll();
#if UNITY_WEBGL && !UNITY_EDITOR
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
        ApplyVcaVolume(musicVCA, MusicVolume);
        PlayerPrefs.SetFloat(PREFIX + "music_vol", MusicVolume);
        Notify();
    }

    public void SetSfxVolume(int linear01)
    {
        SfxVolume = Mathf.Clamp01(linear01 * 0.1f);
        ApplyVcaVolume(sfxVCA, SfxVolume);
        PlayerPrefs.SetFloat(PREFIX + "sfx_vol", SfxVolume);
        Notify();
    }

    public void SetAmbianceVolume(int linear01)
    {
        AmbianceVolume = Mathf.Clamp01(linear01 * 0.1f);
        ApplyVcaVolume(ambianceVCA, AmbianceVolume);
        PlayerPrefs.SetFloat(PREFIX + "ambiance_vol", AmbianceVolume);
        Notify();
    }

    private void ApplyVcaVolume(VCA vca, float linearVolume)
    {
        if (!vca.isValid()) return;
        vca.setVolume(linearVolume); // le VCA in FMOD vogliono un valore lineare 0-1, non dB
    }

    /// <summary>Riporta tutte le impostazioni ai valori di default.</summary>
    public void ResetSettingsToDefaults()
    {
        ApplyAll();
        Notify();
    }

    // ==================== LOAD / APPLY ====================
    public void ApplyAll()
    {
        ApplyVcaVolume(musicVCA, MusicVolume);
        ApplyVcaVolume(sfxVCA, SfxVolume);
        ApplyVcaVolume(ambianceVCA, AmbianceVolume);
    }

    private void Notify() => OnSettingsChanged?.Invoke();
}