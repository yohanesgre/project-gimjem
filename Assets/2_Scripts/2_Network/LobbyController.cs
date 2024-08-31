// using System.Collections;
// using System.Collections.Generic;
// using GimJem.Core;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

// namespace GimJem.Network
// {
//     public class LobbyController : MonoBehaviour
//     {
//         [SerializeField] private Button createRoomButton;
//         [SerializeField] private Button joinRoomButton;
//         [SerializeField] private InputField roomKeyInput;
//         [SerializeField] private Text statusText;
//         [SerializeField] private Button readyButton;
//         [SerializeField] private Button startGameButton;
//         [SerializeField] private Transform playerListContent;
//         [SerializeField] private GameObject playerListItemPrefab;
//         [SerializeField] private GameObject loadingScreen;
//         [SerializeField] private Text loadingText;

//         private Dictionary<string, bool> playerReadyStatus = new Dictionary<string, bool>();

//         private async void Start()
//         {
//             // Connect to Nakama server
//             await NakamaNetworkManager.Instance.Connect(SystemInfo.deviceUniqueIdentifier);

//             createRoomButton.onClick.AddListener(CreateRoom);
//             joinRoomButton.onClick.AddListener(JoinRoom);
//             readyButton.onClick.AddListener(ToggleReady);
//             startGameButton.onClick.AddListener(StartGame);

//             // Initially hide lobby UI
//             SetLobbyUIActive(false);

//             NakamaNetworkManager.Instance.OnPlayerJoined += OnPlayerJoined;
//             NakamaNetworkManager.Instance.OnPlayerLeft += OnPlayerLeft;
//             NakamaNetworkManager.Instance.OnPlayerReady += OnPlayerReady;
//             NakamaNetworkManager.Instance.OnGameStarted += OnGameStarted;
//             NakamaNetworkManager.Instance.OnGameStarted += OnGameStarted;
//             NakamaNetworkManager.Instance.OnAllPlayersLoaded += OnAllPlayersLoaded;

//             loadingScreen.SetActive(false);
//         }

//         private async void CreateRoom()
//         {
//             statusText.text = "Creating room...";
//             try
//             {
//                 string roomKey = await NakamaNetworkManager.Instance.CreateRoom();
//                 statusText.text = $"Room created. Key: {roomKey}";
//                 SetLobbyUIActive(true);
//                 UpdatePlayerList();
//             }
//             catch (System.Exception e)
//             {
//                 statusText.text = $"Failed to create room: {e.Message}";
//             }
//         }

//         private async void JoinRoom()
//         {
//             string roomKey = roomKeyInput.text;
//             if (string.IsNullOrEmpty(roomKey))
//             {
//                 statusText.text = "Please enter a room key";
//                 return;
//             }

//             statusText.text = "Joining room...";
//             try
//             {
//                 await NakamaNetworkManager.Instance.JoinRoom(roomKey);
//                 statusText.text = "Joined room successfully";
//                 SetLobbyUIActive(true);
//                 UpdatePlayerList();
//             }
//             catch (System.Exception e)
//             {
//                 statusText.text = $"Failed to join room: {e.Message}";
//             }
//         }

//         private void ToggleReady()
//         {
//             bool isReady = !playerReadyStatus[NakamaNetworkManager.Instance.PlayerId];
//             NakamaNetworkManager.Instance.SetPlayerReady(isReady);
//             readyButton.GetComponentInChildren<Text>().text = isReady ? "Unready" : "Ready";
//         }

//         private void StartGame()
//         {
//             if (!NakamaNetworkManager.Instance.IsHost)
//             {
//                 statusText.text = "Only the host can start the game";
//                 return;
//             }

//             if (!AllPlayersReady())
//             {
//                 statusText.text = "Not all players are ready";
//                 return;
//             }

//             NakamaNetworkManager.Instance.StartGame();
//         }

//         private void OnPlayerJoined(string playerId)
//         {
//             playerReadyStatus[playerId] = false;
//             UpdatePlayerList();
//         }

//         private void OnPlayerLeft(string playerId)
//         {
//             playerReadyStatus.Remove(playerId);
//             UpdatePlayerList();
//         }

//         private void OnPlayerReady(string playerId, bool isReady)
//         {
//             playerReadyStatus[playerId] = isReady;
//             UpdatePlayerList();
//         }

//         private void OnGameStarted()
//         {
//             loadingScreen.SetActive(true);
//             loadingText.text = "Loading game...";
//         }

//         private void OnAllPlayersLoaded()
//         {
//             loadingScreen.SetActive(false);
//             // All players are now ready to start the game
//             StartGameplay();
//         }

//         private void StartGameplay()
//         {
//             // Initialize gameplay elements
//             // This method will be called when all players have loaded the scene
//             Debug.Log("All players loaded. Starting gameplay!");

//             // You can now enable player controls, spawn entities, etc.
//         }

//         private void UpdatePlayerList()
//         {
//             foreach (Transform child in playerListContent)
//             {
//                 Destroy(child.gameObject);
//             }

//             foreach (var player in playerReadyStatus)
//             {
//                 GameObject playerItem = Instantiate(playerListItemPrefab, playerListContent);
//                 playerItem.GetComponentInChildren<Text>().text = $"Player: {player.Key} - {(player.Value ? "Ready" : "Not Ready")}";
//             }

//             startGameButton.interactable = NakamaNetworkManager.Instance.IsHost && AllPlayersReady();
//         }

//         private bool AllPlayersReady()
//         {
//             foreach (var playerReady in playerReadyStatus.Values)
//             {
//                 if (!playerReady) return false;
//             }
//             return true;
//         }

//         private void SetLobbyUIActive(bool active)
//         {
//             readyButton.gameObject.SetActive(active);
//             startGameButton.gameObject.SetActive(active && NakamaNetworkManager.Instance.IsHost);
//             playerListContent.gameObject.SetActive(active);
//         }

//     }

// }
