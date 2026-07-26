using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private CinemachineCamera menuCamera;
    [SerializeField] private CinemachineCamera settingsCamera;
    [SerializeField] private CinemachineCamera creditsCamera;
    [SerializeField] private CinemachineCamera leaderboardCamera;
    [SerializeField] private CinemachineCamera gameCamera;
    
    [SerializeField] private MenuButton[] buttons;
    
    [Header("Items")]
    [Tooltip("Item da attivare e disattivare tra inizio e fine scena")]
    public List<GameObject> turnableitems = new List<GameObject>();

    [Header("Pannelli UI")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("TurnDown Items")]
    [SerializeField] List<GameObject> turnDownItems = new List<GameObject>();
    private void Start()
    {
        TurnDownItems();
        ShowMainMenu();
    }

    private void ResetCameras()
    {
        menuCamera.Priority = 5;
        settingsCamera.Priority = 5;
        creditsCamera.Priority = 5;
        leaderboardCamera.Priority = 5;
        gameCamera.Priority = 5;
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }
    void BlockMenuInput()
    {
        foreach (MenuButton button in buttons)
            button.DisableCollider();
    }
    public void Signal_OnPlayClick()
    {
        BlockMenuInput();
        StartCoroutine(PlayCoroutine());
    }

    private IEnumerator PlayCoroutine()
    {
        yield return new WaitForSeconds(1f);
        ResetCameras();
        gameCamera.Priority = 20;
        InputHandler.Instance.CurrentState = InputHandler.State.Game;
        foreach (GameObject gb in turnableitems)
        {
            gb.SetActive(true);
            // Altre implementazioni varie
        }
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
 
    public void Signal_OnSettingsClick()
    {
        BlockMenuInput();
        StartCoroutine(SettingsCoroutine());
    }
    private IEnumerator SettingsCoroutine()
    {
        yield return new WaitForSeconds(1f);
        ResetCameras();
        settingsPanel.SetActive(true);
        settingsCamera.Priority = 20;
    }
 
    public void Signal_OnCreditsClick()
    {
        BlockMenuInput();
        StartCoroutine(CreditsCoroutine());
    }
    private IEnumerator CreditsCoroutine()
    {
        yield return new WaitForSeconds(1f);
        ResetCameras();
        creditsPanel.SetActive(true);
        creditsCamera.Priority = 20;
    }
 
    public void Signal_OnQuitClick()
    {
        BlockMenuInput();
        StartCoroutine(QuitCoroutine());
    }
    private IEnumerator QuitCoroutine()
    {
        yield return new WaitForSeconds(1f);
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
 
    public void ShowMainMenu()
    {
        foreach (MenuButton button in buttons)
            button.EnableCollider();
        ResetCameras();
        menuCamera.Priority = 20;
    }

    public void TurnDownItems()
    {
        foreach (GameObject gb in turnableitems)
        {
            gb.SetActive(false);
        }
    }
}
