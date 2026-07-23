using UnityEngine;

// Script dato come potere al Player.
// Trova item attorno al Player.
// Seleziona quell'item e chiama l'Inventory_Sistem
public class PlayerStealItem : PlayerInteraction
{
    [Range(0f, 1f)] public float stealthSkill = 0.5f;
    [SerializeField] LayerMask itemLayerMask;
    [SerializeField] LayerMask villagerLayerMask;
    [SerializeField] float interactionRadius = 2f;
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
            if ((transform.position - target.transform.position).sqrMagnitude > interactionRadius * interactionRadius) return;
            var itemInfo = target.GetComponent<ItemInfoComponent>().GetInfo();
            if (target.TryGetComponent(out StealableItem item))
            {
                if (itemInfo != null && InventorySystem.Instance.AddInventory(itemInfo))
                {
                    float currentStealth = stealthSkill;
                    item.Steal(transform, Mathf.Clamp01(currentStealth));
                }
            }
        }
    }
}