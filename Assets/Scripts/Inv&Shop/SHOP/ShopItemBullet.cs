using UnityEngine;

public class ShopItemBullet : MonoBehaviour, IShopItem
{
    [SerializeField] private int cost = 10;

    public void Buy(PlayerInteractionShop shop)
    {
        shop.CompraBullet(cost);
    }
}