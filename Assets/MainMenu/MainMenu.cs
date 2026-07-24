using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Items")]
    [Tooltip("Item da attivare e disattivare tra inizio e fine scena")]
    public List<GameObject> turnableitems = new List<GameObject>();

    [Header("Pannelli UI")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
 
    private void Start()
    {
        Time.timeScale = 0f;
        foreach (GameObject gb in turnableitems)
        {
            gb.SetActive(false);
        }
        ShowMainMenu();
    }
 
    public void Signal_OnPlayClick()
    {
        Time.timeScale = 1f;
        foreach (GameObject gb in turnableitems)
        {
            gb.SetActive(true);
            // Altre implementazioni varie
        }
    }
 
    public void Signal_OnSettingsClick()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
 
    public void Signal_OnCreditsClick()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }
 
    public void Signal_OnQuitClick()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // In WebGL non si puo' chiudere la scheda/finestra del browser via codice
        // (per motivi di sicurezza): Application.Quit() non ha alcun effetto qui.
        // Meglio nascondere del tutto il pulsante Esci quando la build e' WebGL,
        // oppure mostrare un messaggio tipo "Puoi chiudere questa scheda per uscire".
        Debug.Log("Quit non disponibile in WebGL: chiudi la scheda del browser per uscire.");
#else
        Application.Quit();
#endif
 
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
 
    /// <summary>Collega questo a un pulsante "Indietro" dentro Settings/Crediti per tornare al menu.</summary>
    public void ShowMainMenu()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
}
