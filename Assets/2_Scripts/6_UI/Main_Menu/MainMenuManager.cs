using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GimJem.Core;
using GimJem.Network;
using GimJem.UI.MainMenu;
using GimJem.Utilities;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public event Action<string> OnPlayerJoined;
    public event Action<string> OnPlayerLeft;
    public event Action<string, bool> OnPlayerReady;
    public event Action OnGameStarted;
    public event Action OnAllPlayersLoaded;
    public event Action<ConnectionStatus> OnConnectionStateUpdated;
    public event Action OnCreateRoom;

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
    private void Start()
    {
        NakamaNetworkManager.Instance.OnPlayerJoined += NetworkOnPlayerJoined;
        NakamaNetworkManager.Instance.OnPlayerLeft += NetworkOnPlayerLeft;
        NakamaNetworkManager.Instance.OnPlayerReady += NetworkOnPlayerReady;
        NakamaNetworkManager.Instance.OnGameStarted += NetworkOnGameStarted;
        NakamaNetworkManager.Instance.OnAllPlayersLoaded += NetworkOnAllPlayersLoaded;
        NakamaNetworkManager.Instance.OnConnectionStateUpdated += NetworkOnConnectionStateUpdated;

        for (int i = 0; i < controllers.Count; i++)
        {
            controllers[i].GetComponent<IUIController<MainMenuManager>>().Init(this);
        }
    }

    private void OnDisable()
    {
        NakamaNetworkManager.Instance.OnPlayerJoined -= NetworkOnPlayerJoined;
        NakamaNetworkManager.Instance.OnPlayerLeft -= NetworkOnPlayerLeft;
        NakamaNetworkManager.Instance.OnPlayerReady -= NetworkOnPlayerReady;
        NakamaNetworkManager.Instance.OnGameStarted -= NetworkOnGameStarted;
        NakamaNetworkManager.Instance.OnAllPlayersLoaded -= NetworkOnAllPlayersLoaded;
        NakamaNetworkManager.Instance.OnConnectionStateUpdated -= NetworkOnConnectionStateUpdated;
    }

    private void NetworkOnPlayerJoined(string playerId)
    {
        OnPlayerJoined?.Invoke(playerId);
    }

    private void NetworkOnPlayerLeft(string playerId)
    {
        OnPlayerLeft?.Invoke(playerId);
    }

    private void NetworkOnPlayerReady(string playerId, bool isReady)
    {
        OnPlayerReady?.Invoke(playerId, isReady);
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

    public async void OnClickConnectToServerButton()
    {
        var deviceId = NakamaNetworkManager.Instance.GetDeviceId();
        await NakamaNetworkManager.Instance.Connect(deviceId);
    }

    public void OnClickCreateRoomButton()
    {

    }

    public async Task CreateRoomAsync()
    {
        try
        {
            string roomKey = await NakamaNetworkManager.Instance.CreateRoom();
            // Copy room key to clipboard
            GUIUtility.systemCopyBuffer = roomKey;
            ToastUtil.ShowSuccessToast($"Room created. Key: {roomKey}\nCopied to clipboard!");
        }
        catch (Exception e)
        {
            ToastUtil.ShowErrorToast($"Failed to create room: {e.Message}");
        }
    }

    public void OpenJoinRoomDialog()
    {
        GetController<DialogJoinRoomController>().Show();
    }

    public void CloseJoinRoomDialog()
    {
        GetController<DialogJoinRoomController>().Hide();
    }

    private T GetController<T>() where T : Component, IUIController<MainMenuManager>
    {
        return controllers.Find(x => x.GetComponent<T>() != null).GetComponent<T>();
    }

    private void OnValidate()
    {
        if (controllers == null || controllers.Count == 0)
        {
            throw new Exception("Controllers list is empty");
        }
    }
}
