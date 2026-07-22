using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Grigios;
public class Inventory_System : Singleton<Inventory_System>
{
    public List<Item_Infos> inventory = new List<Item_Infos>();

    int actualWeight, maxWeight = 200;
    int actualItemCount, maxItemCount = 3;

    int inventoryValue; // Valore monetario dell'inventario
    int moneyHeld; // Soldi che ha effettivamente il player
    public void AddInventory(Item_Infos item)
    {
        if(item.weight <= (maxWeight - actualWeight) && item.countValue <= (maxItemCount - actualItemCount))
        {
            actualWeight += item.weight;
            actualItemCount += item.countValue;
            inventoryValue += item.moneyValue;
            inventory.Add(item);
            item.gameObject.SetActive(false); // Spegni oggetto
        }
        else
        {
            if(item.weight > (maxWeight - actualWeight) && item.countValue <= (maxItemCount - actualItemCount))
            {
                // Troppo pesante l'inventario
            }
            else if(item.countValue > (maxItemCount - actualItemCount) && item.weight <= (maxWeight - actualWeight))
            {
                // Troppi oggetti nell'inventario
            }
            else
            {
                // Tutto ciò che poteva andare storto è andato storto
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
