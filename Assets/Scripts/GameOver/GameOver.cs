using Grigios;
using System.Collections.Generic;
using UnityEngine;
public class GameOver : Singleton<GameOver>
{
    [Header("Turnable Items")]
    [Tooltip("METTERE NPC")]
    public List<GameObject> turnableitems = new List<GameObject>();
    public List<GameObject> turnUpITEMS = new List<GameObject>();
    public void Loose()
    {
        TurnDownItems();
        TurnUpItems();
        LeaderBoardWanted.Instance.SetWanteds();
    }

    public void TurnDownItems()
    {
        foreach (GameObject gb in turnableitems)
        {
            gb.SetActive(false);
        }
    }

    public void TurnUpItems()
    {
        foreach (GameObject gb in turnUpITEMS)
        {
            gb.SetActive(true);
        }
    }
}
