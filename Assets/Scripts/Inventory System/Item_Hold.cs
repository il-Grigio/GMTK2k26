using Unity.VisualScripting;
using UnityEngine;

// Script dato come potere al Player.
// Trova item attorno al Player.
// Seleziona quell'item e chiama l'Inventory_Sistem
public class Item_Hold : MonoBehaviour
{
    public LayerMask targetLayers;

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            // Abilita shining shader dell'oggetto
            
            
        }
    }
}
