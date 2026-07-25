using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [Header("Tab Audio")]
    [SerializeField] private Image[] musicSlider;
    [SerializeField] private Image[] sfxSlider;
    [SerializeField] private Image[] ambianceSlider;
    
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

        SetValue(musicSlider, s.MusicVolume);
        SetValue(sfxSlider, s.SfxVolume);
        SetValue(ambianceSlider, s.AmbianceVolume);

    }

    private void SetValue(Image[] slider, float i)
    {
        for (int j = slider.Length - 1; j >= 0; j--)
        {
            slider[j].enabled = j < i * 10;
        }
    }

    // ---------------- Callback: inoltrano tutto a GameSettings ----------------

    public void OnAmbianceUp() => OnAmbianceSlider(Mathf.Clamp01(GameSettings.Instance.AmbianceVolume + 0.1f));
    public void OnAmbianceDown() => OnAmbianceSlider(Mathf.Clamp01(GameSettings.Instance.AmbianceVolume - 0.1f));
    public void OnMusicUp() => OnMusicSlider(Mathf.Clamp01(GameSettings.Instance.MusicVolume + 0.1f));
    public void OnMusicDown() => OnMusicSlider(Mathf.Clamp01(GameSettings.Instance.MusicVolume - 0.1f));
    public void OnSfxUp() => OnSfxSlider(Mathf.Clamp01(GameSettings.Instance.SfxVolume + 0.1f));
    public void OnSfxDown() => OnSfxSlider(Mathf.Clamp01(GameSettings.Instance.SfxVolume - 0.1f));

    private void OnMusicSlider(float v)
    {
        SetValue(musicSlider, v);
        GameSettings.Instance.SetMusicVolume(v);
    }

    private void OnSfxSlider(float v)
    {
        SetValue(sfxSlider, v);
        GameSettings.Instance.SetSfxVolume(v);
    }

    private void OnAmbianceSlider(float v)
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
