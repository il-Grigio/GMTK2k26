using System.Collections.Generic;
using UnityEngine;

// Il villico ha un raggio di visione, se vede che prendi roba si insospettisce.
// Si insospettisce di pi in base a: peso, valore, itemcount (tutto insomma).
// Range 1: non fa nulla :)
// Range 2: Va a fare pettegolezzi in giro
// Range 3: Chiama lo sceriffo!
public class SuspectVillager : MonoBehaviour
{
    private NPCMovement npcMovement;

    [Header("Sospetto")]
    public int actualSuspectLevel;
    public int[] rangeSuspects;

    [Header("Motivazioni di sospetto (modulare)")]
    [SerializeField] private List<SuspicionReason> suspicionReasons;

    [Header("Vista")]
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Transform eyes;

    [Header("Pettegolezzo")]
    [SerializeField] private int gossipSuspicionAmount = 5; // quanto sospetto trasmette parlando

    // Chiamato solo se il villico � risultato "vicino" tramite OverlapSphere

    private void Start()
    {
        npcMovement = GetComponent<NPCMovement>();
    }

    public void TryDetectTheft(ItemInfoData item, GameObject thief)
    {
        if (!CanSee(thief.transform)) return;

        int totalSuspicion = 0;
        for (int i = 0; i < suspicionReasons.Count; i++)
        {
            totalSuspicion += suspicionReasons[i].CalculateSuspicion(item);
        }

        AggiungiSospetto(totalSuspicion);
    }

    private bool CanSee(Transform target)
    {
        Vector3 origin = eyes != null ? eyes.position : transform.position;
        Vector3 dirToTarget = target.position - origin;

        float angle = Vector3.Angle(transform.forward, dirToTarget);
        if (angle > viewAngle * 0.5f)
            return false;

        if (Physics.Raycast(origin, dirToTarget.normalized, out RaycastHit hit, dirToTarget.magnitude, obstacleMask))
        {
            if (hit.transform != target)
                return false; // qualcosa blocca la vista
        }

        return true;
    }

    public void AggiungiSospetto(int m)
    {
        actualSuspectLevel += m;
        CheckRange();
    }

    private void CheckRange()
    {
        if (rangeSuspects.Length < 3) return;

        if (actualSuspectLevel >= rangeSuspects[2])
        {
            // Chiama lo sceriffo!
        }
        else if (actualSuspectLevel >= rangeSuspects[1])
        {
            // Va a fare pettegolezzi, e se trova qualcuno gli passa un po' di sospetto
            NPCMovement target = npcMovement?.TryStartTalkingWithNearbyNPC();

            if (target != null)
            {
                SuspectVillager targetSuspect = target.GetComponent<SuspectVillager>();
                targetSuspect?.AggiungiSospetto(gossipSuspicionAmount);
            }
        }
        // else: Range 1, non fa nulla
    }
}