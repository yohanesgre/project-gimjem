using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;

public class BakiakGameplayManager : MonoBehaviour
{
    [SerializeField] private BakiakCharRenderer charRenderer;
    [SerializeField] private GameObject[] playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] finishLines;
    private List<BakiakPlayerController> players = new List<BakiakPlayerController>();
    private int playerCount = 1;
    public Action<int> OnPlayerMove;
    public Action<int> OnGameFinished;
    public event Action<int, char> OnCorrectInput;

    [SerializeField] private int firstToWin = 1;
    [SerializeField] private int playerWonIndex = -1;

    private void Start()
    {
        SpawnPlayers();
        charRenderer.Initialize(playerCount);
    }

    private void Update()
    {
        if (IsAnyPlayerFirstToWin())
        {
            OnGameFinished?.Invoke(playerWonIndex);
            return;
        }

        if (IsAnyPlayerFinished())
        {

            StartCoroutine(IE_Refresh_Level());
            return;
        }
    }

    private void SpawnPlayers()
    {
        if (playerPrefab == null || spawnPoints.Length == 0 || finishLines.Length == 0)
        {
            Debug.LogError("Player prefab, spawn points, or finish lines not set!");
            return;
        }

        playerCount = Mathf.Max(1, playerPrefab.Length);

        for (int i = 0; i < playerCount; i++)
        {
            GameObject playerObj = Instantiate(playerPrefab[i], spawnPoints[i].position, spawnPoints[i].rotation);
            BakiakPlayerController player = playerObj.GetComponent<BakiakPlayerController>();
            if (player != null)
            {
                player.Initialize(this, spawnPoints[i].position, finishLines[i].position, i);
                players.Add(player);
            }
        }

        Debug.Log($"Spawned {players.Count} players");
    }


    public void HandleCorrectInput(int playerIndex, char correctChar)
    {
        Debug.Log($"Correct input: {correctChar}");
        OnCorrectInput?.Invoke(playerIndex, correctChar);
    }


    public BakiakPlayerController GetPlayer(int index)
    {
        if (index >= 0 && index < players.Count)
        {
            return players[index];
        }
        return null;
    }

    public int GetPlayerCount()
    {
        return players.Count;
    }

    public bool IsAnyPlayerFinished()
    {
        return players.Any(player => player.IsFinished);
    }

    public bool IsAnyPlayerFirstToWin()
    {
        var result = false;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].PlayerScore == firstToWin)
            {
                playerWonIndex = i;
                result = true;
                break;
            }
        }
        return result;
    }

    private IEnumerator IE_Refresh_Level()
    {
        yield return new WaitForSeconds(1);
        players.ForEach(player => player.ResetPlayer());
    }
}