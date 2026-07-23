using System.Collections.Generic;
using UnityEngine;

public class SaloonManager : MonoBehaviour
{
    public static SaloonManager Instance { get; private set; }

    [Header("Reputazione Player")]
    [Tooltip("0 = nessuno sospetta di te, 1 = tutti ti tengono d'occhio")]
    [Range(0f, 1f)] public float playerHeat = 0f;
    public float heatDecayPerSecond = 0.01f; // quanto scende nel tempo se non fai altri casini
    public float heatGainOnCaught = 0.35f;   // quanto sale se un NPC ti becca direttamente

    private readonly List<SaloonNPC> npcs = new List<SaloonNPC>();

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
        playerHeat = Mathf.Max(0f, playerHeat - heatDecayPerSecond * Time.deltaTime);
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
        playerHeat = Mathf.Clamp01(playerHeat + amount);
    }

    public float GetPlayerHeat() => playerHeat;

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
        return candidates[Random.Range(0, candidates.Count)];
    }
}
