using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using GimJem.Network;

public class BakiakGameplayManager : MonoBehaviour
{
    [SerializeField] private NakamaNetworkManager nakamaNetworkManager;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] finishLines;
    [SerializeField] private int maxPlayers = 4;
    private List<BakiakPlayerController> players = new List<BakiakPlayerController>();
    private int finishedPlayersCount = 0;

    public Action<int> OnPlayerMove;
    public Action<int> OnGameFinished;
    public event Action<int, char> OnCorrectInput;
    private int currentPlayerIndex = 0;

    private void Start()
    {
        SpawnPlayers();
    }


    private void SpawnPlayers()
    {
        if (playerPrefab == null || spawnPoints.Length == 0 || finishLines.Length == 0)
        {
            Debug.LogError("Player prefab, spawn points, or finish lines not set!");
            return;
        }

        int playerCount = Mathf.Min(maxPlayers, spawnPoints.Length, finishLines.Length);

        for (int i = 0; i < playerCount; i++)
        {
            GameObject playerObj = Instantiate(playerPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            BakiakPlayerController player = playerObj.GetComponent<BakiakPlayerController>();
            if (player != null)
            {
                player.Initialize(this, finishLines[i].position, i);
                player.OnPlayerFinished += HandlePlayerFinished;
                players.Add(player);
            }
        }

        Debug.Log($"Spawned {players.Count} players");
    }


    public void HandleCorrectInput(char correctChar)
    {
        Debug.Log($"Correct input: {correctChar}");
        OnCorrectInput?.Invoke(currentPlayerIndex, correctChar);

        // Move to the next player (you may want to implement your own logic here)
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
    }

    private void HandlePlayerFinished(int playerIndex)
    {
        finishedPlayersCount++;
        Debug.Log($"Player {playerIndex} finished! ({finishedPlayersCount}/{players.Count})");

        if (finishedPlayersCount == players.Count)
        {
            OnGameFinished?.Invoke(players.Count);
            Debug.Log("All players have finished!");
        }
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
}