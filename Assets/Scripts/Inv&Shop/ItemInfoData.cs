using UnityEngine;

public class ItemInfoData
{
    public int Weight { get; private set; }
    public int CountValue { get; private set; }
    public int MoneyValue { get; private set; }
    public ItemInfoData(int weight, int countValue, int moneyValue )
    {
        this.Weight = weight;
        this.CountValue = countValue;
        this.MoneyValue = moneyValue;
    }

}
