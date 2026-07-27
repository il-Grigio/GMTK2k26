using UnityEngine;

public class ItemInfoComponent : MonoBehaviour
{
    [SerializeField] int countValue;

    [SerializeField] int moneyValue;
    public int GetPointValue()
    {
        return moneyValue * countValue ;
    }
    public ItemInfoData GetInfo()
    {
        return new ItemInfoData(countValue, moneyValue);
    }

}