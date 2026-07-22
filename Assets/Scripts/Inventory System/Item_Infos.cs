using UnityEditor.UI;
using UnityEngine;

public class Item_Infos : MonoBehaviour
{
    // Mantiene le informazioni utili per un oggetto, classe generica
    // Concettualmente nell'inventario mettiamo un numero massimo di "tre"
    // Inoltre c'è un sistema di peso

    [SerializeField] GameObject gameObject;

    public int weight;

    public int countValue;

    public int moneyValue;
}
