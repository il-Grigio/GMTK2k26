/*
using Dan.Main;
using Dan.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Leaderboard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private List<TextMeshProUGUI> names;
    [SerializeField] private List<TextMeshProUGUI> scores;
    [SerializeField] private TMP_InputField username;
    [SerializeField] TMP_Text playerScore;
    public int score;
    [SerializeField] private int entriesCount;

    private void Awake()
    {
        username.characterLimit = 16;
    }


    private void Start() { 
        InvokeRepeating(nameof(GetLeaderboard), 0f, 30f);
        score = PointSystem.Instance.GetScore();
        playerScore.text = "Your Bounty: " + score.ToString();
    }

    public void GetLeaderboard()
    {
        Leaderboards.GIOCO.GetEntries(OnEntriesLoaded, OnEntriesError);
    }

    public void UploadEntry()
    {
        string cleanName = username.text.Trim();

        if (string.IsNullOrWhiteSpace(cleanName))
        {
            Debug.LogWarning("Username non valido");
            return;
        }

        if (cleanName.Length > 16)
            cleanName = cleanName.Substring(0, 16);

        // Prima controlliamo se esiste già
        Leaderboards.GIOCO.GetEntries((entries) =>
        {

            // Se arriviamo qui, il nome è unico → procediamo
            Leaderboards.GIOCO.UploadNewEntry(cleanName, score, (success) =>
            {
                if (success)
                    GetLeaderboard();
            }, OnEntriesError);

        }, OnEntriesError);
    }

    public void UploadEntryPROVA()
    {
        string cleanName = "kiograh";

        if (string.IsNullOrWhiteSpace(cleanName))
        {
            Debug.LogWarning("Username non valido");
            return;
        }

        if (cleanName.Length > 16)
            cleanName = cleanName.Substring(0, 16);

        // Prima controlliamo se esiste già
        Leaderboards.GIOCO.GetEntries((entries) =>
        {

            // Se arriviamo qui, il nome è unico → procediamo
            Leaderboards.GIOCO.UploadNewEntry(cleanName, 1000000, (success) =>
            {
                if (success)
                    GetLeaderboard();
            }, OnEntriesError);

        }, OnEntriesError);
    }


    public void GetEntriesCount()
    {
        Leaderboards.GIOCO.GetEntryCount((count) =>
        {
            entriesCount = count;
        }, OnEntriesError);
    }

    private void OnEntriesLoaded(Entry[] entries)
    {
        foreach (var n in names) n.text = "Loading...";
        foreach (var s in scores) s.text = "";

        for (int i = 0; i < names.Count; i++)
        {
            if(i < entries.Length)
            {
                names[i].text = entries[i].Username;
                scores[i].text = entries[i].Score.ToString();

                if (entries[i].Username == username.text)
                {
                    names[i].color = Color.yellow; // Praticamente facciamo il cazzo che vogliamo
                    scores[i].color = Color.yellow;
                }
                else
                {
                    names[i].color = Color.white;
                    scores[i].color = Color.white;
                }
            }
            else
            {
                names[i].text = "";
                scores[i].text = "";
            }
        }
    }

    private void OnEntriesError(string error)
    {
        Debug.Log(error);
    }
}
*/