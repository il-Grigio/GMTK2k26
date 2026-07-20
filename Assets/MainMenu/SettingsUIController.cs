using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [Header("Tab Audio")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle musicMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;
    
    private bool isRefreshingUI = false; // evita loop quando aggiorniamo la UI a mano

    private void OnEnable()
    {
        if (GameSettings.Instance == null)
        {
            Debug.LogWarning("GameSettings non trovato in scena. Aggiungi un oggetto con GameSettings prima della schermata Settings.");
            return;
        }
        RefreshUIFromSettings();
        HookEvents();
    }

    private void OnDisable()
    {
        UnhookEvents();
    }

    // ---------------- Riempi la UI con lo stato corrente ----------------

    private void RefreshUIFromSettings()
    {
        var s = GameSettings.Instance;
        isRefreshingUI = true;

        if (musicSlider != null) musicSlider.SetValueWithoutNotify(s.MusicVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(s.SfxVolume);
        if (musicMuteToggle != null) musicMuteToggle.SetIsOnWithoutNotify(s.MusicMuted);
        if (sfxMuteToggle != null) sfxMuteToggle.SetIsOnWithoutNotify(s.SfxMuted);

        isRefreshingUI = false;
    }

    // ---------------- Hook eventi ----------------

    private void HookEvents()
    {
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicSlider);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxSlider);
        if (musicMuteToggle != null) musicMuteToggle.onValueChanged.AddListener(OnMusicMute);
        if (sfxMuteToggle != null) sfxMuteToggle.onValueChanged.AddListener(OnSfxMute);
    }

    private void UnhookEvents()
    {
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicSlider);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxSlider);
        if (musicMuteToggle != null) musicMuteToggle.onValueChanged.RemoveListener(OnMusicMute);
        if (sfxMuteToggle != null) sfxMuteToggle.onValueChanged.RemoveListener(OnSfxMute);
    }

    // ---------------- Callback: inoltrano tutto a GameSettings ----------------

    private void OnMusicSlider(float v) { if (!isRefreshingUI) GameSettings.Instance.SetMusicVolume(v); }
    private void OnSfxSlider(float v) { if (!isRefreshingUI) GameSettings.Instance.SetSfxVolume(v); }
    private void OnMusicMute(bool v) { if (!isRefreshingUI) GameSettings.Instance.SetMusicMuted(v); }
    private void OnSfxMute(bool v) { if (!isRefreshingUI) GameSettings.Instance.SetSfxMuted(v); }

    /// <summary>Collega questo a un pulsante "Ripristina predefiniti" nel Canvas.</summary>
    public void OnResetToDefaultsPressed()
    {
        GameSettings.Instance.ResetSettingsToDefaults();
        RefreshUIFromSettings();
    }
}
