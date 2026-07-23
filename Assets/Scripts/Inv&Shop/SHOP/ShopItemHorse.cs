using UnityEngine;

public class ShopItemHorse : MonoBehaviour, IShopItem
{
    [SerializeField] private int cost = 50;

    public void Buy(PlayerInteractionShop shop)
    {
        shop.CompraCavallo(cost);
    }
}