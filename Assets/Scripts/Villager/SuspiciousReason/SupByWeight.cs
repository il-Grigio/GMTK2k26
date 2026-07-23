using UnityEngine;
[CreateAssetMenu(menuName = "Suspicion/By Weight")]

public class SupByWeight : SuspicionReason
{
    public float multiplier = 1f;

    public override int CalculateSuspicion(ItemInfoData item)
    {
        return Mathf.RoundToInt(item.Weight * multiplier);
    }
}