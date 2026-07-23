using UnityEngine;

// Script dato come potere al Player.
// Trova item attorno al Player.
// Seleziona quell'item e chiama l'Inventory_Sistem
public class PlayerInteractionsItem : PlayerInteraction
{
    [SerializeField] LayerMask itemLayerMask;
    [SerializeField] LayerMask villagerLayerMask;
    [SerializeField] float suspicionCheckRadius = 20f; // deve coprire il raggio di vista max dei villici

    protected override void OnEnable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= ItemToInventory;
        }
        _input.OnInteractAction += ItemToInventory;
    }

    protected override void OnDisable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= ItemToInventory;
        }
    }

    private void ItemToInventory(Vector2 mousePos)
    {
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f, itemLayerMask))
        {
            var target = hitInfo.collider.gameObject;
            var itemInfo = target.GetComponent<ItemInfoComponent>().GetInfo();

            if (itemInfo != null && Inventory_System.Instance.AddInventory(itemInfo))
            {
                target.SetActive(false);

                TheftNotifier.NotifyTheft(
                    itemInfo,
                    target.transform.position,
                    gameObject,
                    suspicionCheckRadius,
                    villagerLayerMask
                );
            }
        }
    }
}