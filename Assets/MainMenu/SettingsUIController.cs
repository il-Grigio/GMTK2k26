using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [Header("Tab Audio")]
    [SerializeField] private Image[] musicSlider;
    [SerializeField] private Image[] sfxSlider;
    [SerializeField] private Image[] ambianceSlider;
    
    int currentMusicVolume;
    int currentSfxVolume;
    int currentAmbianceVolume;
    private bool isRefreshingUI = false; // evita loop quando aggiorniamo la UI a mano

    private void OnEnable()
    {
        if (GameSettings.Instance == null)
        {
            Debug.LogWarning("GameSettings non trovato in scena. Aggiungi un oggetto con GameSettings prima della schermata Settings.");
            return;
        }
        RefreshUIFromSettings();
    }


    // ---------------- Riempi la UI con lo stato corrente ----------------

    private void RefreshUIFromSettings()
    {
        var s = GameSettings.Instance;

        currentMusicVolume = Mathf.RoundToInt(s.MusicVolume * 10);
        currentSfxVolume = Mathf.RoundToInt(s.SfxVolume * 10);
        currentAmbianceVolume = Mathf.RoundToInt(s.AmbianceVolume * 10);
        
        SetValue(musicSlider, currentMusicVolume);
        SetValue(sfxSlider, currentSfxVolume);
        SetValue(ambianceSlider, currentAmbianceVolume);

    }

    private void SetValue(Image[] slider, int i)
    {
        for (int j = slider.Length - 1; j >= 0; j--)
        {
            slider[j].enabled = j < i;
        }
    }

    // ---------------- Callback: inoltrano tutto a GameSettings ----------------

    public void OnAmbianceUp() => OnAmbianceSlider(Mathf.Min(currentAmbianceVolume + 1, 10));
    public void OnAmbianceDown() => OnAmbianceSlider(Mathf.Max(currentAmbianceVolume - 1, 0));
    public void OnMusicUp() => OnMusicSlider(Mathf.Min(currentMusicVolume + 1, 10));
    public void OnMusicDown() => OnMusicSlider(Mathf.Max(currentMusicVolume - 1, 0));
    public void OnSfxUp() => OnSfxSlider(Mathf.Min(currentSfxVolume + 1, 10));
    public void OnSfxDown() => OnSfxSlider(Mathf.Max(currentSfxVolume - 1, 0));

    private void OnMusicSlider(int v)
    {
        currentMusicVolume = v; // aggiungi questa riga per tenere lo state coerente col valore clampato
        SetValue(musicSlider, v);
        GameSettings.Instance.SetMusicVolume(v);
    }

    private void OnSfxSlider(int v)
    {
        currentSfxVolume = v;
        SetValue(sfxSlider, v);
        GameSettings.Instance.SetSfxVolume(v);
    }

    private void OnAmbianceSlider(int v)
    {
        currentAmbianceVolume = v;
        SetValue(ambianceSlider, v);
        GameSettings.Instance.SetAmbianceVolume(v);
    }

    /// <summary>Collega questo a un pulsante "Ripristina predefiniti" nel Canvas.</summary>
    public void OnResetToDefaultsPressed()
    {
        GameSettings.Instance.ResetSettingsToDefaults();
        RefreshUIFromSettings();
    }
}
