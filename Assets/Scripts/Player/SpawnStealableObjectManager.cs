using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Grigios;
public class SpawnStealableObjectManager : Singleton<SpawnStealableObjectManager>
{
    [Header("LISTA DI OGGETTI DA RUBARE")]
    // [SerializeField] List<GameObject> stealable = new List<GameObject>();
    [SerializeField] List<GameObject> stoleItem = new List<GameObject>();

    [Header("Tempo di Respawn")]
    [SerializeField] float minRespawn = 5f;
    [SerializeField] float maxRespawn = 30f;
    private float timer;
    public void TurnDownItems(GameObject stealableItem)
    {
        // stealable.Remove(stealableItem);
        stoleItem.Add(stealableItem);
        stealableItem.gameObject.SetActive(false);
        if(timer <= 0) timer = Random.Range(minRespawn, maxRespawn);
    }

    public void TurnOnItems(GameObject stealableItem) {
        if (stoleItem.Count == 0) return;

        // stealable.Add(stealableItem);
        stoleItem.Remove(stealableItem);
        stealableItem.SetActive(true);
        if(timer <= 0) timer = Random.Range(minRespawn, maxRespawn);
    }

    private void Update()
    {
        if (timer > 0) {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 0;
            if(stoleItem.Count > 0) 
                TurnOnItems(stoleItem[Random.Range(0, stoleItem.Count)]);
        }
    }

}
