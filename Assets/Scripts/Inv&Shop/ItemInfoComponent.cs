using UnityEditor.UI;
using UnityEngine;

public class ItemInfoComponent : MonoBehaviour
{
    [SerializeField] GameObject gameObject;

    [SerializeField] int weight;

    [SerializeField] int countValue;

    [SerializeField] int moneyValue;

    public int GetPointValue()
    {
        Debug.Log("moneyvalue: " + moneyValue + " countvalue: " + countValue + " moneyvalue: " + moneyValue);
        return moneyValue * countValue + weight;
    }
    public ItemInfoData GetInfo()
    {
        return new ItemInfoData(weight, countValue, moneyValue);
    }
}