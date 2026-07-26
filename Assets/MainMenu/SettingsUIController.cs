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

    public void OnAmbianceUp() => OnAmbianceSlider(++currentAmbianceVolume);
    public void OnAmbianceDown() => OnAmbianceSlider(--currentAmbianceVolume);
    public void OnMusicUp() => OnMusicSlider(++currentMusicVolume);
    public void OnMusicDown() => OnMusicSlider(--currentMusicVolume);
    public void OnSfxUp() => OnSfxSlider(++currentSfxVolume);
    public void OnSfxDown() => OnSfxSlider(--currentSfxVolume);

    private void OnMusicSlider(int v)
    {
        Debug.Log(v);
        SetValue(musicSlider, v);
        GameSettings.Instance.SetMusicVolume(v);
    }

    private void OnSfxSlider(int v)
    {
        SetValue(sfxSlider, v);
        GameSettings.Instance.SetSfxVolume(v);
    }

    private void OnAmbianceSlider(int v)
    {
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
