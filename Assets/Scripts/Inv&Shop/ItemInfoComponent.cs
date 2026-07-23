using UnityEditor.UI;
using UnityEngine;

public class ItemInfoComponent : MonoBehaviour
{
    [SerializeField] GameObject gameObject;

    [SerializeField] int weight;

    [SerializeField] int countValue;

    [SerializeField] int moneyValue;

    public ItemInfoData GetInfo()
    {
        return new ItemInfoData(weight, countValue, moneyValue);
    }
}