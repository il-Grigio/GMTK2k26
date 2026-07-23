using UnityEngine;

public class PlayerInteractionShop : PlayerInteraction
{
    // Liberare cavalli, comprare proiettili o comprare dinamite 
    // Deve parlare con l'inventory system per aggiungere gli oggetti acquistati
    // Deve parlare con l'inventory system per rimuovere i soldi spesi

    [SerializeField] LayerMask shopObject;
    [SerializeField] float interactionRadius = 4;
    protected override void OnEnable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= Click;
        }
        _input.OnInteractAction += Click;
    }

    protected override void OnDisable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= Click;
        }
    }

    void Click(Vector2 mousePos)
    {
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f, shopObject))
        {
            if (hitInfo.collider.TryGetComponent<IShopItem>(out var item))
            {
                if ((transform.position - hitInfo.transform.position).sqrMagnitude > interactionRadius * interactionRadius) return;
                item.Buy(this);
            }
        }
    }

    public void BuyHorse(int m)
    {
        if (SpendiSoldi(m))
        {
            PointSystem.Instance.cavalliRaccolti += 1;
            // Instantiate(cavallo, cavalloSpawnPoint.position, Quaternion.identity);
        }

    }

    public void BuyBullet(int m)
    {
        if (SpendiSoldi(m))
        {
        }
    }

    public void BuyDynamite() // Non so se sia necessario, ma nel caso lo sia, lo aggiungo
    {

    }

    public bool SpendiSoldi(int m)
    {
        return Inventory_System.Instance.RemoveMoneyCheck(m);
    }
    
}
