using UnityEngine;

public abstract class SuspicionReason : ScriptableObject
{
    public string reasonName;

    public abstract int CalculateSuspicion(ItemInfoData item);
}