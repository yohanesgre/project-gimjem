using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GimJem.Utilities;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using EasyUI.Toast;

namespace GimJem.Network
{
    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Failed,
    }

    public class NakamaNetworkManager : MonoBehaviour
    {
        public event System.Action<string, bool> OnPlayerJoined;
        public event System.Action<string, bool> OnPlayerLeft;
        public event System.Action<string, bool> OnPlayerReady;
        public event System.Action OnRoomCreated;
        public event System.Action OnGameStarted;
        public event System.Action OnAllPlayersLoaded;
        public event System.Action<ConnectionStatus> OnConnectionStateUpdated;

        private int playersLoadedCount = 0;
        private int totalPlayers = 0;
        private const int MAX_PLAYERS = 1;

        [Header("Nakama Server Settings")]
        [SerializeField] private string SCHEME = "http"; // Replace with your Nakama server address scheme
        [SerializeField] private string HOST = "http://localhost"; // Replace with your Nakama server address
        [SerializeField] private int PORT = 7350; // Replace with your Nakama server port
        [SerializeField] private string SERVER_KEY = "defaultkey"; // Replace with your server key

        [Header("Debug")]
        [ReadOnly][SerializeField] private bool _isHost;
        public bool IsHost { get => _isHost; private set => _isHost = value; }

        [ReadOnly][SerializeField] private string _playerId;
        public string PlayerId { get => _playerId; private set => _playerId = value; }

        [ReadOnly][SerializeField] private string _roomKey;
        public string RoomKey { get => _roomKey; private set => _roomKey = value; }

        private IClient client;
        private ISession session;
        private ISocket socket;
        private IMatch currentMatch;

        public static NakamaNetworkManager Instance { get; private set; }

        public bool IsRoomFull => totalPlayers == MAX_PLAYERS;

        #region Unity Lifecycle Related
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            socket?.CloseAsync();
        }
        #endregion

        #region Server Related

        /// <summary>
        /// Initializes the Nakama client with the provided server settings.
        /// </summary>
        /// <remarks>
        /// This method sets up the Nakama client with the specified scheme, host, port, and server key.
        /// The client timeout is set to 5 seconds.
        /// </remarks>
        public void InitConnectionClient()
        {
            try
            {
                client = new Client(SCHEME, HOST, PORT, SERVER_KEY)
                {
                    Timeout = 5
                };
                RegisterMatchPresencesListeners();
                RegisterMatchStateListeners();
            }
            catch (System.Exception e)
            {
                ToastUtil.ShowErrorToast($"Failed to initialize Nakama client: {e.Message}");
            }
        }

        public string GetDeviceId()
        {
            // If the user's device ID is already stored, grab that - alternatively get the System's unique device identifier.
            var deviceId = PlayerPrefs.GetString("deviceId", SystemInfo.deviceUniqueIdentifier);

            // If the device identifier is invalid then let's generate a unique one.
            if (deviceId == SystemInfo.unsupportedIdentifier)
            {
                deviceId = System.Guid.NewGuid().ToString();
            }

            return deviceId;
        }


        /// <summary>
        /// Connects to the Nakama server and socket.
        /// </summary>
        /// <param name="deviceId">The device ID to use for authentication.</param>
        /// <remarks>
        /// This will authenticate the device with the Nakama server and connect to the socket.
        /// </remarks>
        public async Task Connect(string deviceId)
        {
            try
            {
                ToastUtil.ShowSuccessToast("Try connect to server...");
                OnConnectionStateUpdated?.Invoke(ConnectionStatus.Connecting);

                // Save the user's device ID to PlayerPrefs so it can be retrieved during a later play session for re-authenticating.
                PlayerPrefs.SetString("deviceId", deviceId);

                session = await client.AuthenticateDeviceAsync(deviceId);
                Debug.Log("Connected to Nakama server");

                socket = client.NewSocket();
                await socket.ConnectAsync(session);
                Debug.Log("Connected to Nakama socket");

                PlayerId = session.UserId;

                await Task.Delay(2000);
                OnConnectionStateUpdated?.Invoke(ConnectionStatus.Connected);
                // Show success Toast
                ToastUtil.ShowSuccessToast("Successfully connected to server");
            }
            catch (System.Exception e)
            {
                OnConnectionStateUpdated?.Invoke(ConnectionStatus.Failed);
                ToastUtil.ShowErrorToast($"Failed to connect: {e.Message}");
            }
        }
        #endregion

        #region Room Related
        /// <summary>
        /// Creates a new room on the Nakama server.
        /// </summary>
        /// <returns>The room key created.</returns>
        public async Task CreateRoom()
        {
            var match = await socket.CreateMatchAsync();
            currentMatch = match;
            IsHost = true;
            RoomKey = match.Id;
            Debug.Log($"Created room with key: {RoomKey}");

            OnRoomCreated?.Invoke();
        }

        /// <summary>
        /// Joins an existing room on the Nakama server.
        /// </summary>
        /// <param name="roomKey">The key of the room to join.</param>
        /// <exception cref="System.Exception">Failed to join room.</exception>
        public async Task JoinRoom(string roomKey)
        {
            try
            {
                var match = await socket.JoinMatchAsync(roomKey);
                currentMatch = match;
                IsHost = false;
                RoomKey = roomKey;
                Debug.Log($"Joined room with key: {RoomKey}");


            }
            catch (System.Exception e)
            {
                ToastUtil.ShowErrorToast($"Failed to join room: {e.Message}");
            }
        }

        /// <summary>
        /// Attempts to join a room with the given room key.
        /// If the room is full and the player is not the host, they will automatically leave the room.
        /// </summary>
        /// <param name="roomKey">The key of the room to join.</param>
        public async Task TryJoinRoom(string roomKey)
        {
            // Join the room
            await JoinRoom(roomKey);

            // If the room is full and the player is not the host, leave the room
            if (!IsHost && IsRoomFull)
            {
                await LeaveRoom();
            }
        }

        /// <summary>
        /// Leaves the current room on the Nakama server.
        /// </summary>
        public async Task LeaveRoom()
        {
            await socket.LeaveMatchAsync(currentMatch.Id);
            currentMatch = null;
            IsHost = false;
            RoomKey = null;
            Debug.Log("Left room");
        }

        /// <summary>
        /// Registers listeners for match presence events (i.e. players joining or leaving).
        /// </summary>
        private void RegisterMatchPresencesListeners()
        {
            socket.ReceivedMatchPresence += match =>
            {
                // Queue the processing on the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    foreach (var presence in match.Joins)
                    {
                        OnPlayerJoined?.Invoke(presence.UserId, presence.UserId == PlayerId);
                    }
                    foreach (var presence in match.Leaves)
                    {
                        OnPlayerLeft?.Invoke(presence.UserId, presence.UserId == PlayerId);
                    }
                });
            };
        }

        /// <summary>
        /// Registers listeners for match state events, specifically for handling player ready states and game start events.
        /// </summary>
        public void RegisterMatchStateListeners()
        {
            socket.ReceivedMatchState += matchState =>
            {
                if (matchState.OpCode == NetworkConstants.LOBBY_READY_OP_CODE) // Ready state update
                {
                    var readyState = JsonParser.FromJson<Dictionary<string, bool>>(ConvertByteArrayToString(matchState.State));
                    OnPlayerReady?.Invoke(matchState.UserPresence.UserId, readyState["IsReady"]);
                }
                else if (matchState.OpCode == NetworkConstants.LOBBY_GAME_START_OP_CODE) // Game start
                {
                    var startGameState = JsonParser.FromJson<Dictionary<string, object>>(ConvertByteArrayToString(matchState.State));
                    totalPlayers = System.Convert.ToInt32(startGameState["TotalPlayers"]);
                    LoadGameplayScene();
                }
                else if (matchState.OpCode == NetworkConstants.LOBBY_PLAYER_LOADED_OP_CODE) // Player loaded
                {
                    playersLoadedCount++;
                    if (playersLoadedCount == totalPlayers)
                    {
                        OnAllPlayersLoaded?.Invoke();
                    }
                }
            };
        }

        /// <summary>
        /// Sets the ready state of the player in the current match.
        /// </summary>
        /// <param name="isReady">Whether the player is ready or not.</param>
        /// <remarks>
        /// This will send a state update to the Nakama server, which will be received by other players in the match.
        /// </remarks>
        public void SetPlayerReady(bool isReady)
        {
            if (currentMatch == null) return;

            var readyState = new { IsReady = isReady };
            string stateJson = JsonWriter.ToJson(readyState);
            var opCode = NetworkConstants.LOBBY_READY_OP_CODE; // Use opcode 3 for ready state updates
            socket.SendMatchStateAsync(currentMatch.Id, opCode, stateJson);
        }

        /// <summary>
        /// Sends a state update to the Nakama server, which will be received by other players in the match, to start the game.
        /// </summary>
        /// <remarks>
        /// Only the host player can start the game.
        /// This will send a message to the Nakama server, which will be received by other players in the match.
        /// </remarks>
        public void StartGame()
        {
            if (currentMatch == null || !IsHost) return;

            var startGameState = new { StartGame = true };
            string stateJson = JsonWriter.ToJson(startGameState);
            var opCode = NetworkConstants.LOBBY_GAME_START_OP_CODE; // Use opcode 4 for game start
            socket.SendMatchStateAsync(currentMatch.Id, opCode, stateJson);
        }

        public void TryStartGame()
        {
            if (totalPlayers > MAX_PLAYERS && totalPlayers < 1) return;

            StartGame();
        }
        #endregion

        public void SendPlayerState(PlayerNetworkState playerState)
        {
            if (currentMatch == null) return;

            string stateJson = JsonWriter.ToJson(playerState);
            var opCode = 1; // Use opcode 1 for player state updates
            socket.SendMatchStateAsync(currentMatch.Id, opCode, stateJson);
        }

        public void BroadcastWorldState(WorldNetworkState worldState)
        {
            if (currentMatch == null || !IsHost) return;

            string stateJson = JsonWriter.ToJson(worldState);
            var opCode = 2; // Use opcode 2 for world state updates
            socket.SendMatchStateAsync(currentMatch.Id, opCode, stateJson);
        }

        public void RegisterPlayerStateHandler(System.Action<PlayerNetworkState> onPlayerStateReceived)
        {
            socket.ReceivedMatchState += matchState =>
            {
                if (matchState.OpCode == 1) // Check if it's a player state update
                {
                    var playerState = JsonParser.FromJson<PlayerNetworkState>(ConvertByteArrayToString(matchState.State));
                    onPlayerStateReceived(playerState);
                }
            };
        }

        public void RegisterWorldStateHandler(System.Action<WorldNetworkState> onWorldStateReceived)
        {
            socket.ReceivedMatchState += matchState =>
            {
                if (matchState.OpCode == 2) // Check if it's a world state update
                {
                    var worldState = JsonParser.FromJson<WorldNetworkState>(ConvertByteArrayToString(matchState.State));
                    onWorldStateReceived(worldState);
                }
            };
        }

        #region Scene Related
        /// <summary>
        /// Loads the gameplay scene asynchronously.
        /// When the scene is finished loading, it calls <see cref="OnSceneLoaded"/>.
        /// </summary>
        private void LoadGameplayScene()
        {
            SceneManager.LoadSceneAsync("GameplayScene").completed += OnSceneLoaded;
        }


        /// <summary>
        /// Called when the gameplay scene is finished loading.
        /// Sends a notification to other players that this player has loaded the scene.
        /// </summary>
        /// <param name="op">The asynchronous operation that loaded the scene.</param>
        private void OnSceneLoaded(AsyncOperation op)
        {
            // Notify other players that this player has loaded the scene
            var playerLoadedState = new { PlayerLoaded = true };
            string stateJson = JsonWriter.ToJson(playerLoadedState);
            var opCode = NetworkConstants.LOBBY_PLAYER_LOADED_OP_CODE; // Use opcode 5 for player loaded notification
            socket.SendMatchStateAsync(currentMatch.Id, opCode, stateJson);
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Converts a given byte array to a UTF-8 encoded string.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>The converted string.</returns>
        private string ConvertByteArrayToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
        #endregion


    }
}
