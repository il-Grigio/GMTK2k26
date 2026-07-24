using UnityEngine;
using Grigios;
public class PointSystem : Singleton<PointSystem>
{
    [SerializeField] private float score;

    [Header("Score Manager")]
    [SerializeField]float scoreMultiplier = 1.0f;
    [SerializeField]float timeToIncreaseMultiplier = 10.0f;
    [SerializeField]float scoreMultiplierIncrease = 0.1f;
    
    public void AddScore(int qta)
    {
        if(timeToIncreaseMultiplier > 0)
        {
            AddScoreMultiplier(scoreMultiplierIncrease);
            timeToIncreaseMultiplier = 10.0f;
        }
        AddPlainScore(qta);
    }
    public void AddScoreNOCombo(int qta)
    {
        AddPlainScoreNOMultiplier(qta);
    }

    private void AddPlainScore(float qta)
    {
        score += (qta * scoreMultiplier);
    }

    private void AddPlainScoreNOMultiplier(float qta)
    {
        score += qta;
    }   

    private void AddScoreMultiplier(float multiplier)
    {
        scoreMultiplier += multiplier;
    }

    public int GetScore()
    {
        return (int)score;
    }

    private void Update()
    {
        if (timeToIncreaseMultiplier > 0)
            timeToIncreaseMultiplier -= Time.deltaTime;
        else
            scoreMultiplier = 1.0f;
    }
}

