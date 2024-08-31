using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TugWarGameplayManager : MonoBehaviour
{
    [SerializeField] private RopeController ropeController;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] finishLines;
    [SerializeField] private int maxPlayers = 1; // Set to 1 to control only the first player
    [SerializeField] private List<GameObject> players = new List<GameObject>();

    public Action<char> OnCorrectInput;
    public Action OnGameFinished;

    [SerializeField] private float stepSize = 0.01f; // Step size for movement

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

        for (int i = 0; i < maxPlayers; i++)
        {
            GameObject playerObj = Instantiate(playerPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            TugWarPlayerController player = playerObj.GetComponent<TugWarPlayerController>();
            if (player != null)
            {
                player.Initialize(spawnPoints[i].position, finishLines[i]);
                players.Add(playerObj);
            }
        }

        Debug.Log($"Spawned {players.Count} player(s)");
    }

    public void HandleCorrectInput(char correctChar)
    {
        OnCorrectInput?.Invoke(correctChar);
        if (players.Count > 0 && ropeController != null)
        {
            // Move the rope
            TugWarPlayerController firstPlayer = players[0].GetComponent<TugWarPlayerController>();
            Vector3 directionToPlayer = (firstPlayer.GetPosition() - ropeController.transform.position).normalized;
            Vector3 movementDirection = new Vector3(-directionToPlayer.x, 0f, -directionToPlayer.z).normalized;
            ropeController.MoveInDirection(movementDirection * stepSize);

            for (int i = 0; i < players.Count; i++)
            {
                players[i].GetComponent<TugWarPlayerController>().UpdateCenterPoint(spawnPoints[i].position);
            }
        }
    }

    public TugWarPlayerController GetPlayer()
    {
        return players.Count > 0 ? players[0].GetComponent<TugWarPlayerController>() : null;
    }

    public bool IsPlayerFinished()
    {
        return players.Count > 0 && players[0].GetComponent<TugWarPlayerController>().IsFinished;
    }

    public void HandleObjectExitRaycastZone(GameObject obj)
    {
        Debug.Log($"Object exited: {obj.name}");
        if (obj == players[0].GetComponent<TugWarPlayerController>().GetFinishLine().gameObject)
        {
            SetPlayerWin();
        }
    }

    private void SetPlayerWin()
    {
        if (players.Count > 0 && !players[0].GetComponent<TugWarPlayerController>().IsFinished)
        {
            players[0].GetComponent<TugWarPlayerController>().SetFinished();
            Debug.Log("Player has won!");
            OnGameFinished?.Invoke();
        }
    }

    public bool IsAnyPlayerFinished()
    {
        return players.Any(player => player.GetComponent<TugWarPlayerController>().IsFinished);
    }
}