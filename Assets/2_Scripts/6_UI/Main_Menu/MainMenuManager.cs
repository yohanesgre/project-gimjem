using System;
using System.Collections;
using System.Collections.Generic;
using GimJem.Core;
using GimJem.Network;
using GimJem.Utilities;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private NakamaNetworkManager nakamaNetworkManager;

    [Header("Controllers")]
    public List<GameObject> controllers;

    private void Awake()
    {
        nakamaNetworkManager = NakamaNetworkManager.Instance;
        if (nakamaNetworkManager == null)
        {
            throw new Exception("NakamaNetworkManager is not found");
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < controllers.Count; i++)
        {
            controllers[i].GetComponent<IController<MainMenuManager>>().Init(this);
        }

        NakamaNetworkManager.Instance.OnPlayerJoined += OnPlayerJoined;
        NakamaNetworkManager.Instance.OnPlayerLeft += OnPlayerLeft;
        NakamaNetworkManager.Instance.OnPlayerReady += OnPlayerReady;
        NakamaNetworkManager.Instance.OnGameStarted += OnGameStarted;
        NakamaNetworkManager.Instance.OnGameStarted += OnGameStarted;
        NakamaNetworkManager.Instance.OnAllPlayersLoaded += OnAllPlayersLoaded;
        NakamaNetworkManager.Instance.OnConnectionStateUpdated += OnConnectionStateUpdated;
    }

    private void OnDisable()
    {
        NakamaNetworkManager.Instance.OnPlayerJoined -= OnPlayerJoined;
        NakamaNetworkManager.Instance.OnPlayerLeft -= OnPlayerLeft;
        NakamaNetworkManager.Instance.OnPlayerReady -= OnPlayerReady;
        NakamaNetworkManager.Instance.OnGameStarted -= OnGameStarted;
        NakamaNetworkManager.Instance.OnGameStarted -= OnGameStarted;
        NakamaNetworkManager.Instance.OnAllPlayersLoaded -= OnAllPlayersLoaded;
        NakamaNetworkManager.Instance.OnConnectionStateUpdated -= OnConnectionStateUpdated;
    }

    private void OnPlayerJoined(string playerId)
    {
        Debug.Log($"Player {playerId} joined the game");
    }

    private void OnPlayerLeft(string playerId)
    {
        Debug.Log($"Player {playerId} left the game");
    }

    private void OnPlayerReady(string playerId, bool isReady)
    {
        Debug.Log($"Player {playerId} is ready: {isReady}");
    }

    private void OnGameStarted()
    {
        Debug.Log("Game started");
    }

    private void OnAllPlayersLoaded()
    {
        Debug.Log("All players loaded");
    }

    private void OnConnectionStateUpdated(ConnectionStatus status)
    {
        Debug.Log($"Connection state updated: {status}");
    }

    public async void OnClickConnectToServerButton()
    {
        var deviceId = NakamaNetworkManager.Instance.GetDeviceId();
        await NakamaNetworkManager.Instance.Connect(deviceId);
    }

    public async void OnClickCreateRoomButton()
    {
        try
        {
            string roomKey = await NakamaNetworkManager.Instance.CreateRoom();
            ToastUtil.ShowSuccessToast($"Room created. Key: {roomKey}");

        }
        catch (System.Exception e)
        {
            ToastUtil.ShowErrorToast($"Failed to create room: {e.Message}");
        }
    }

    private void OnValidate()
    {
        if (controllers == null || controllers.Count == 0)
        {
            throw new System.Exception("Controllers list is empty");
        }
    }
}
