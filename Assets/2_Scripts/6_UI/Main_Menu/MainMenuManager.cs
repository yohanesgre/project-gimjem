using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.UI.MainMenu;
using GimJem.Core;
using GimJem.Network;
using GimJem.UI.MainMenu;
using GimJem.Utilities;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public event Action<string, bool, bool, int> OnPlayerJoined;
    public event Action<string, bool, int> OnPlayerLeft;
    public event Action<string, bool, bool> OnPlayerReadyChanged;
    public event Action OnGameStarted;
    public event Action OnAllPlayersLoaded;
    public event Action<ConnectionStatus> OnConnectionStateUpdated;
    public event Action OnRoomCreated;

    private NakamaNetworkManager nakamaNetworkManager;

    [Header("Controllers")]
    public List<GameObject> controllers;

    private void Start()
    {
        nakamaNetworkManager = NakamaNetworkManager.Instance;
        if (nakamaNetworkManager == null)
        {
            throw new Exception("NakamaNetworkManager is not found");
        }

        NakamaNetworkManager.Instance.OnPlayerJoined += NetworkOnPlayerJoined;
        NakamaNetworkManager.Instance.OnPlayerLeft += NetworkOnPlayerLeft;
        NakamaNetworkManager.Instance.OnPlayerReady += NetworkOnPlayerReady;
        NakamaNetworkManager.Instance.OnRoomCreated += NetworkOnRoomCreated;
        NakamaNetworkManager.Instance.OnGameStarted += NetworkOnGameStarted;
        NakamaNetworkManager.Instance.OnAllPlayersLoaded += NetworkOnAllPlayersLoaded;
        NakamaNetworkManager.Instance.OnConnectionStateUpdated += NetworkOnConnectionStateUpdated;

        for (int i = 0; i < controllers.Count; i++)
        {
            controllers[i].GetComponent<IUIController<MainMenuManager>>().Init(this);
        }
    }

    private void OnDestroy()
    {
        if (NakamaNetworkManager.Instance != null)
        {
            NakamaNetworkManager.Instance.OnPlayerJoined -= NetworkOnPlayerJoined;
            NakamaNetworkManager.Instance.OnPlayerLeft -= NetworkOnPlayerLeft;
            NakamaNetworkManager.Instance.OnPlayerReady -= NetworkOnPlayerReady;
            NakamaNetworkManager.Instance.OnRoomCreated -= NetworkOnRoomCreated;
            NakamaNetworkManager.Instance.OnGameStarted -= NetworkOnGameStarted;
            NakamaNetworkManager.Instance.OnAllPlayersLoaded -= NetworkOnAllPlayersLoaded;
            NakamaNetworkManager.Instance.OnConnectionStateUpdated -= NetworkOnConnectionStateUpdated;
        }
    }

    private void NetworkOnPlayerJoined(string playerId, bool isSelf)
    {
        Debug.Log($"{GetType().Name} | NetworkOnPlayerJoined | Player {playerId} joined. IsSelf: {isSelf}");
        OnPlayerJoined?.Invoke(playerId, isSelf, nakamaNetworkManager.IsHost, 1);
    }

    private void NetworkOnPlayerLeft(string playerId, bool isSelf)
    {
        OnPlayerLeft?.Invoke(playerId, nakamaNetworkManager.IsHost, 1);
    }

    private void NetworkOnPlayerReady(string playerId, bool isReady)
    {
        OnPlayerReadyChanged?.Invoke(playerId, isReady, nakamaNetworkManager.IsHost);
    }

    private void NetworkOnRoomCreated()
    {
        OnRoomCreated?.Invoke();

        // Copy room key to clipboard
        GUIUtility.systemCopyBuffer = nakamaNetworkManager.RoomKey;
        ToastUtil.ShowSuccessToast($"Room created. Key: {nakamaNetworkManager.RoomKey}\nCopied to clipboard!");
    }

    private void NetworkOnGameStarted()
    {
        OnGameStarted?.Invoke();
    }

    private void NetworkOnAllPlayersLoaded()
    {
        OnAllPlayersLoaded?.Invoke();
    }

    private void NetworkOnConnectionStateUpdated(ConnectionStatus status)
    {
        OnConnectionStateUpdated?.Invoke(status);
    }

    public async void ConnectToServer()
    {
        var deviceId = NakamaNetworkManager.Instance.GetDeviceId();
        await nakamaNetworkManager.Connect(deviceId);
    }

    public async Task CreateRoomAsync()
    {
        try
        {
            await nakamaNetworkManager.CreateRoom();
        }
        catch (Exception e)
        {
            ToastUtil.ShowErrorToast($"Failed to create room: {e.Message}");
        }
    }

    public async void JoinRoom(string roomKey)
    {
        try
        {
            ToastUtil.ShowSuccessToast($"Joining room...");
            await nakamaNetworkManager.TryJoinRoom(roomKey);
            ToastUtil.ShowSuccessToast($"Joined room {roomKey}");
        }
        catch (Exception e)
        {
            ToastUtil.ShowErrorToast($"Failed to join room: {e.Message}");
        }
    }

    public void StartGame()
    {
        nakamaNetworkManager.TryStartGame();
    }

    public async void LeaveRoom()
    {
        await nakamaNetworkManager.LeaveRoom();
    }

    public void OpenJoinRoomDialog()
    {
        GetController<DialogJoinRoomController>().Show();
    }

    public void CloseJoinRoomDialog()
    {
        GetController<DialogJoinRoomController>().Hide();
    }

    public void OpenLobbyRoomView()
    {
        GetController<LobbyRoomController>().Show();
    }

    public void CloseLobbyRoomView()
    {
        GetController<LobbyRoomController>().Hide();
    }

    private T GetController<T>() where T : Component, IUIController<MainMenuManager>
    {
        return controllers.Find(x => x.GetComponent<T>() != null).GetComponent<T>();
    }

    private GameObject GetGameObjectOfType<T>() where T : Component, IUIController<MainMenuManager>
    {
        return controllers.Find(x => x.GetComponent<T>() != null);
    }

    private void OnValidate()
    {
        if (controllers == null || controllers.Count == 0)
        {
            throw new Exception("Controllers list is empty");
        }
    }


}
