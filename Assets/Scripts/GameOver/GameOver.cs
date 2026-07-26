using Grigios;
using System.Collections.Generic;
using UnityEngine;
public class GameOver : Singleton<GameOver>
{
    [Header("Turnable Items")]
    [Tooltip("METTERE NPC")]
    public List<GameObject> turnableitems = new List<GameObject>(); // 

    public void Loose()
    {
        TurnDownItems();
    }

    public void TurnDownItems()
    {
        foreach (GameObject gb in turnableitems)
        {
            gb.SetActive(false);
        }
    }
}
