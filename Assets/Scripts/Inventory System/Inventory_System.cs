using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Grigios;
public class Inventory_System : Singleton<Inventory_System>
{
    public List<ItemInfoData> inventory = new List<ItemInfoData>();

    [SerializeField] int actualWeight, maxWeight = 200;
    [SerializeField] int actualItemCount, maxItemCount = 3;
    [SerializeField] int inventoryValue; // Valore monetario dell'inventario
    [SerializeField] int moneyHeld; // Soldi che ha effettivamente il player
    public bool AddInventory(ItemInfoData item)
    {
        if(item.Weight <= (maxWeight - actualWeight) && item.CountValue <= (maxItemCount - actualItemCount))
        {
            actualWeight += item.Weight;
            actualItemCount += item.CountValue;
            inventoryValue += item.MoneyValue;
            inventory.Add(item);
            return true;
        }
        else
        {
            if(item.Weight > (maxWeight - actualWeight) && item.CountValue <= (maxItemCount - actualItemCount))
            {
                // Troppo pesante l'inventario
                return false;
            }
            else if(item.CountValue > (maxItemCount - actualItemCount) && item.Weight <= (maxWeight - actualWeight))
            {
                // Troppi oggetti nell'inventario
                return false;
            }
            else
            {
                // Tutto ciò che poteva andare storto è andato storto
                return false;
            }
        }
    }

    public void SellInventory()
    {
        AddMoney(inventoryValue);

        inventoryValue = 0;

        actualWeight = 0;

        actualItemCount = 0;

        inventory.Clear();
    }


    public void AddMoney(int m)
    {
        moneyHeld += m;
    }

    public bool RemoveMoneyCheck(int m) // Controlla se effettivamente hai i soldi per comprare
    {
        if(moneyHeld - m < 0)
        {
            return false;
        }

        RemoveMoney(m);
        return true;
    }

    private void RemoveMoney(int m)
    {
        moneyHeld -= m;
    }
}
