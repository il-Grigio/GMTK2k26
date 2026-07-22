using UnityEditor.UI;
using UnityEngine;

public class ItemInfoComponent : MonoBehaviour
{
    // Mantiene le informazioni utili per un oggetto, classe generica
    // Concettualmente nell'inventario mettiamo un numero massimo di "tre"
    // Inoltre c'è un sistema di peso

    [SerializeField] GameObject gameObject;

    [SerializeField] int weight;

    [SerializeField] int countValue;

    [SerializeField] int moneyValue;

    public ItemInfoData GetInfo()
    {
        return new ItemInfoData(weight, countValue, moneyValue);
    }
}