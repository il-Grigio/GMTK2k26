using Unity.VisualScripting;
using UnityEngine;
using Grigios;
using TMPro;
public class TimerSystem : Grigios.Singleton<TimerSystem>
{
    [Header("Timer Settings")]
    [SerializeField] private float timer = 40f;
    [SerializeField] float increaseTime = 10f;
    [SerializeField] TextMeshProUGUI text;
    private bool isStopped = true;
    [SerializeField] GameObject duelObject;

    public float GetTimer()
    {
        return timer;
    }

    public void IncreaseTimer(float multiplier)
    {
        timer += increaseTime * multiplier * 0.8f;
    }

    public void ReduceTimer(int qta)
    {
        timer -= qta;
    }

    private void Update()
    {
        if (timer > 0 && !isStopped)
        {
            timer -= Time.deltaTime;

            int totalSeconds = Mathf.CeilToInt(timer);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            text.text = $"{minutes:00}:{seconds:00}";
        }
        else if (timer > 0 && isStopped)
        {
            
        }
        else
        {
            timer = 0;
            text.text = "00:00";
            increaseTime = 0;
            duelObject.SetActive(true);
            DuelManager.Instance.PrepareDuel();
            //Debug.Log("TEMPO FINITO");
        }
    }

    public void PauseTimer()
    {
        isStopped = true;
    }

    public void ResumeTimer() {
        isStopped = false;
    }
}

