using TMPro;
using UnityEngine;
using Grigios;

public class LeaderBoardWanted : Singleton<LeaderBoardWanted>
{
    [SerializeField] private TextMeshProUGUI[] names = new TextMeshProUGUI[4];
    [SerializeField] private TextMeshProUGUI[] scores = new TextMeshProUGUI[4];
    [SerializeField] private int maxEntries = 10;

    public void SetWanteds()
    {
        var leaderboard = dreamloLeaderBoard.Instance;

        if (leaderboard == null)
        {
            Debug.LogError("dreamloLeaderBoard non trovato in scena.");
            return;
        }

        var scoreList = leaderboard.ToListHighToLow();

        // Numero di slot disponibili a schermo (il minore tra i due array, per sicurezza)
        int slotCount = Mathf.Min(names.Length, scores.Length);

        if (scoreList == null || scoreList.Count == 0)
        {
            for (int i = 0; i < slotCount; i++)
            {
                names[i].text = "";
                scores[i].text = "";
            }
            return;
        }

        int count = Mathf.Min(slotCount, Mathf.Min(maxEntries, scoreList.Count));

        for (int i = 0; i < slotCount; i++)
        {
            if (i < count)
            {
                names[i].text = scoreList[i].playerName;
                scores[i].text = scoreList[i].score.ToString();
            }
            else
            {
                // Svuota gli slot in eccesso se ci sono meno punteggi degli slot disponibili
                names[i].text = "";
                scores[i].text = "";
            }
        }
    }
}