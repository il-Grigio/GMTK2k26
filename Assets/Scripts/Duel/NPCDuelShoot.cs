using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPCDuelShoot : MonoBehaviour
{
    
    void OnEnable()
    {
        DuelManager.Instance.OnCountDown -= CountDown;
        DuelManager.Instance.OnCountDown += CountDown;
    }

    private void OnDisable()
    {
        DuelManager.Instance.OnCountDown -= CountDown;
    }

    private void CountDown(int i)
    {
        if (i > 0)
        {
            Step();
        }
        else
        {
            Invoke(nameof(Shoot), Random.Range(0.25f, 0.7f));
        }
    }

    private void Shoot()
    {
        DuelManager.Instance.OnNPCShoot();
    }
    private void Step()
    {
        transform.position += transform.forward * 1f;
    }
}
