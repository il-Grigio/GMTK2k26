using UnityEngine;

public static class TheftNotifier
{
    private static readonly Collider[] buffer = new Collider[32]; // capienza max villici rilevabili contemporaneamente

    public static void NotifyTheft(ItemInfoData item, Vector3 stealPosition, GameObject thief, float detectionRadius, LayerMask villagerMask)
    {
        int count = Physics.OverlapSphereNonAlloc(stealPosition, detectionRadius, buffer, villagerMask);

        for (int i = 0; i < count; i++)
        {
            if (buffer[i].TryGetComponent<SuspectVillager>(out var villager))
            {
                villager.TryDetectTheft(item, thief);
            }
        }
    }
}