using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaloonManager : MonoBehaviour
{
    public static SaloonManager Instance { get; private set; }
    [Header("Reputazione Player")]
    [Tooltip("0 = nessuno sospetta di te, 1 = tutti ti tengono d'occhio")]
    [Range(0f, 1f)] public float playerHeat = 0f;
    public float heatDecayPerSecond = 0.01f; // quanto scende nel tempo se non fai altri casini
    public float heatGainOnCaught = 0.35f;   // quanto sale se un NPC ti becca direttament
    [Header("Update UI")]
    [SerializeField] Image[] stelleImage = new Image[5]; // componenti Image delle stelle
    [SerializeField] float heatPerStar = 0.18f; // quanto heat serve per accendere una stella

    [Header("Music State")]
    [Tooltip("Soglie di playerHeat oltre le quali si passa allo stato musicale successivo")]
    [SerializeField] float[] musicHeatThresholds = { 0.20f, 0.50f, 0.70f };
    private int lastMusicState = -1;

    public event Action<float> OnHeatChanged;

    private readonly List<SaloonNPC> npcs = new List<SaloonNPC>();
    private int lastStarCount = -1; // tiene traccia del rank attuale, per rilevare i cambi
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Update()
    {
        SetHeat(Mathf.Max(0f, playerHeat - heatDecayPerSecond * Time.deltaTime));
    }
    public void Register(SaloonNPC npc)
    {
        if (!npcs.Contains(npc)) npcs.Add(npc);
    }
    public void Unregister(SaloonNPC npc)
    {
        npcs.Remove(npc);
    }
    // ---------------- REPUTAZIONE PLAYER ----------------
    public void IncreasePlayerHeat(float amount)
    {
        SetHeat(Mathf.Clamp01(playerHeat + amount));
    }
    public float GetPlayerHeat() => playerHeat;

    private void SetHeat(float newHeat)
    {
        playerHeat = newHeat;

        int currentStarCount = Mathf.Clamp(Mathf.FloorToInt(playerHeat / heatPerStar), 0, stelleImage.Length);
        if (currentStarCount != lastStarCount)
        {
            lastStarCount = currentStarCount;
            OnHeatChanged?.Invoke(playerHeat);
            UpdateStars(currentStarCount);
        }

        UpdateMusicState();
    }

    // Determina lo stato musicale in base alle soglie di heat e lo applica solo se cambiato
    private void UpdateMusicState()
    {
        int newState = 0;
        for (int i = 0; i < musicHeatThresholds.Length; i++)
        {
            if (playerHeat >= musicHeatThresholds[i])
                newState = i + 1;
        }

        if (newState != lastMusicState)
        {
            lastMusicState = newState;
            AudioManager.Instance.SetMusicState(newState);
        }
    }

    // Accende/spegne le stelle in base al rank corrente. Chiamato solo quando il rank cambia.
    private void UpdateStars(int starsToLight)
    {
        for (int i = 0; i < stelleImage.Length; i++)
        {
            if (stelleImage[i] == null) continue;
            stelleImage[i].enabled = i < starsToLight;
        }
    }

    // ---------------- FURTO / SPARATORIE ----------------
    // Chiamato dal sistema di furto: notifica tutti gli NPC abbastanza vicini
    public void NotifyTheft(Transform thief, float stealthValue, Vector3 theftPosition)
    {
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.IsAlive) continue;
            npc.OnTheftAttempt(thief, stealthValue, theftPosition);
        }
    }
    // Chiamato quando avviene una sparatoria, per avvisare i testimoni vicini
    public void BroadcastShooting(SaloonNPC shooter, SaloonNPC victim, Vector3 position, float witnessRadius = 12f)
    {
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.IsAlive || npc == shooter || npc == victim) continue;
            float dist = Vector3.Distance(npc.transform.position, position);
            if (dist <= witnessRadius)
            {
                npc.OnWitnessShooting(shooter, victim);
            }
        }
    }
    // ---------------- RICERCA NPC ----------------
    public List<SaloonNPC> GetNPCsNear(Vector3 position, float radius, SaloonNPC exclude = null)
    {
        List<SaloonNPC> result = new List<SaloonNPC>();
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.IsAlive || npc == exclude) continue;
            if (Vector3.Distance(npc.transform.position, position) <= radius)
                result.Add(npc);
        }
        return result;
    }
    // Usato per il "colpo a vuoto": trova un bersaglio casuale nel raggio, diverso da chi spara/mira originale
    public SaloonNPC GetRandomNearby(Vector3 position, float radius, params SaloonNPC[] exclude)
    {
        List<SaloonNPC> candidates = new List<SaloonNPC>();
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.IsAlive) continue;
            bool excluded = false;
            foreach (var ex in exclude)
            {
                if (npc == ex) { excluded = true; break; }
            }
            if (excluded) continue;
            if (Vector3.Distance(npc.transform.position, position) <= radius)
                candidates.Add(npc);
        }
        if (candidates.Count == 0) return null;
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
}