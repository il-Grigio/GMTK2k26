using Unity.VisualScripting;
using UnityEngine;

// Script dato come potere al Player.
// Trova item dove clicka il player.
// Seleziona quell'item e chiama l'Inventory_Sistem

public class PlayerInteractionSelling : PlayerInteraction
{
    [SerializeField] LayerMask sellingPlace;

    protected override void OnEnable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= SellInventory;
        }
        _input.OnInteractAction += SellInventory;
    }

    protected override void OnDisable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= SellInventory;
        }
    }

    private void SellInventory(Vector2 mousePos)
    {
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f, sellingPlace))
        {
            Inventory_System.Instance.SellInventory();
        }
    }

}
