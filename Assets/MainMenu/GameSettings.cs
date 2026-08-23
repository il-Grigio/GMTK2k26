using System.Collections;
using System;
using FMODUnity;
using FMOD.Studio;
using Grigios;
using UnityEngine;

public class GameSettings : Singleton<GameSettings>
{
    [Header("FMOD VCA Paths")]
    [SerializeField] private string musicVcaPath = "vca:/MUSIC";
    [SerializeField] private string sfxVcaPath = "vca:/SFX";
    [SerializeField] private string ambianceVcaPath = "vca:/AMBIANCE";

    private VCA musicVCA;
    private VCA sfxVCA;
    private VCA ambianceVCA;

    public float MusicVolume { get; private set; } = 0.70f;
    public float SfxVolume { get; private set; } = 0.70f;
    public float AmbianceVolume { get; private set; } = 0.70f;

    public event Action OnSettingsChanged;
    private const string PREFIX = "gs_";
    private bool vcaReady = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(WaitForBanksAndInit());
    }

    private IEnumerator WaitForBanksAndInit()
    {
        // Aspetta finché tutte le bank non sono caricate (fondamentale su WebGL)
        while (!RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        musicVCA = RuntimeManager.GetVCA(musicVcaPath);
        sfxVCA = RuntimeManager.GetVCA(sfxVcaPath);
        ambianceVCA = RuntimeManager.GetVCA(ambianceVcaPath);

        vcaReady = true;
        ApplyAll();
    }

    public void SetMusicVolume(int linear01)
    {
        MusicVolume = Mathf.Clamp01(linear01 * 0.1f);
        if (vcaReady) ApplyVcaVolume(musicVCA, MusicVolume);
        PlayerPrefs.SetFloat(PREFIX + "music_vol", MusicVolume);
        Notify();
    }

    // ... SetSfxVolume / SetAmbianceVolume identici, con lo stesso controllo vcaReady

    private void ApplyVcaVolume(VCA vca, float linearVolume)
    {
        if (!vca.isValid()) return;
        vca.setVolume(linearVolume);
    }

    public void ResetSettingsToDefaults()
    {
        if (vcaReady) ApplyAll();
        Notify();
    }

    public void ApplyAll()
    {
        ApplyVcaVolume(musicVCA, MusicVolume);
        ApplyVcaVolume(sfxVCA, SfxVolume);
        ApplyVcaVolume(ambianceVCA, AmbianceVolume);
    }

    private void Notify() => OnSettingsChanged?.Invoke();
}