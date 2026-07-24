using Unity.VisualScripting;
using UnityEngine;
using Grigios;
public class TimerSystem : Grigios.Singleton<TimerSystem>
{
    [Header("Timer Settings")]
    [SerializeField] private float timer = 30f;
    [SerializeField] float increaseTime = 10f;

    public float GetTimer()
    {
        return timer;
    }

    public void IncreaseTimer(float multiplier)
    {
        timer += increaseTime * multiplier;
    }

    public void ReduceTimer(int qta)
    {
        timer -= qta;
    }

    private void Update()
    {
        if (timer > 0)
        {
            
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 0;
            increaseTime = 0;
            Debug.Log("TEMPO FINITO");
        }
    }
}

