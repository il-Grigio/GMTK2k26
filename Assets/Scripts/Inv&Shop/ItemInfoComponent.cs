using UnityEngine;
using UnityEngine.Serialization;

public class ItemInfoComponent : MonoBehaviour
{
    [FormerlySerializedAs("countValue")] [SerializeField] int weightValue;

    [SerializeField] int moneyValue;
    public int GetPointValue()
    {
        return moneyValue * weightValue ;
    }
    public ItemInfoData GetInfo()
    {
        return new ItemInfoData(weightValue, moneyValue);
    }

}