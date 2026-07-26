using UnityEngine;
using Grigios;
using TMPro;
public class PointSystem : Singleton<PointSystem>
{
    [SerializeField] private float score;

    [Header("Score Manager")]
    [SerializeField]float scoreMultiplier = 1.0f;
    [SerializeField]float timeToIncreaseMultiplier = 10.0f;
    [SerializeField]float scoreMultiplierIncrease = 0.1f;
    [SerializeField] TextMeshProUGUI bounty;
    public void AddScore(int qta)
    {
        Debug.Log(qta + "!!!!!!");
        if (timeToIncreaseMultiplier > 0)
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
        bounty.text = score.ToString();
        Debug.Log("Score: " + score + " Multiplier: " + scoreMultiplier);
    }

    private void AddPlainScoreNOMultiplier(float qta)
    {
        score += qta;
        bounty.text = score.ToString();
    }

    private void AddScoreMultiplier(float multiplier)
    {
        scoreMultiplier += multiplier;
    }

    public int GetScore()
    {
        return (int)score;
    }

    public void DoubleScore()
    {
        score *= 2;
        bounty.text = score.ToString();
    }
    private void Update()
    {
        if (timeToIncreaseMultiplier > 0)
            timeToIncreaseMultiplier -= Time.deltaTime;
        else
            scoreMultiplier = 1.0f;
    }

    string playername = "John Doe";
    public void GetPlayerName(string nome)
    {
        playername = nome;
    }
    public void SendScore()
    {
        dreamloLeaderBoard.Instance.AddScore(playername, (int)score);
        LeaderBoardWanted.Instance.SetWanteds();
    }
}

