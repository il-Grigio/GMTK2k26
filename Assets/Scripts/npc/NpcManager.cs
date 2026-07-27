using FMODUnity;
using Grigios;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Grigios;
public class NpcManager : Grigios.Singleton<NpcManager>
{
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

    private readonly List<NPCController> npcs = new List<NPCController>();
    private int lastStarCount = -1; // tiene traccia del rank attuale, per rilevare i cambi
    private void Update()
    {
        SetHeat(Mathf.Max(0f, playerHeat - heatDecayPerSecond * Time.deltaTime));
    }
    public void Register(NPCController npcController)
    {
        if (!npcs.Contains(npcController)) npcs.Add(npcController);
    }
    public void Unregister(NPCController npcController)
    {
        npcs.Remove(npcController);
    }
    // ---------------- REPUTAZIONE PLAYER ----------------
    public void IncreasePlayerHeat(float amount)
    {
        SetHeat(Mathf.Clamp01(playerHeat + amount));
    }
    public float GetPlayerHeat() => playerHeat;

    public void SetHeat(float newHeat)
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
    public void BroadcastShooting(NPCController shooter, NPCController victim, Vector3 position, float witnessRadius = 12f)
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
    // Chiamato quando il PLAYER uccide/colpisce un NPC, per avvisare i testimoni vicini.
    // Diverso da BroadcastShooting perch� qui non c'� un SaloonNPC "shooter".
    public void BroadcastPlayerShooting(NPCController victim, Vector3 position, float witnessRadius = 12f)
    {
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.IsAlive || npc == victim) continue;
            float dist = Vector3.Distance(npc.transform.position, position);
            if (dist <= witnessRadius)
            {
                npc.OnWitnessPlayerShooting(victim);
            }
        }
    }
    // ---------------- RICERCA NPC ----------------
    public List<NPCController> GetNPCsNear(Vector3 position, float radius, NPCController exclude = null)
    {
        List<NPCController> result = new List<NPCController>();
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.IsAlive || npc == exclude) continue;
            if (Vector3.Distance(npc.transform.position, position) <= radius)
                result.Add(npc);
        }
        return result;
    }
    // Usato per il "colpo a vuoto": trova un bersaglio casuale nel raggio, diverso da chi spara/mira originale
    public NPCController GetRandomNearby(Vector3 position, float radius, params NPCController[] exclude)
    {
        List<NPCController> candidates = new List<NPCController>();
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