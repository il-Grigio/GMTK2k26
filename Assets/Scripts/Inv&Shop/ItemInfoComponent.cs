using UnityEditor.UI;
using UnityEngine;

public class ItemInfoComponent : MonoBehaviour
{
    [SerializeField] GameObject gameObject;

    [SerializeField] int countValue;

    [SerializeField] int moneyValue;

    public int GetPointValue()
    {
        Debug.Log("moneyvalue: " + moneyValue + " countvalue: " + countValue + " moneyvalue: " + moneyValue);
        return moneyValue * countValue ;
    }
    public ItemInfoData GetInfo()
    {
        return new ItemInfoData(countValue, moneyValue);
    }
}