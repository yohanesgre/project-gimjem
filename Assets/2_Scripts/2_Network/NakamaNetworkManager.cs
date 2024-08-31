using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GimJem.Utilities;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public event System.Action<string> OnRoomCreated;
        public event System.Action OnLeaveRoom;
        public event System.Action OnGameStarted;
        public event System.Action OnAllPlayersLoaded;
        public event System.Action<ConnectionStatus> OnConnectionStateUpdated;
        [SerializeField, ReadOnly] private int totalPlayersJoinedRoom = 0;
        [SerializeField, ReadOnly] private int playersLoadedCarnivalCount = 0;
        private const int MAX_PLAYERS = 4;

        [Header("Nakama Server Settings")]
        [SerializeField] private string SCHEME = "http"; // Replace with your Nakama server address scheme
        [SerializeField] private string HOST = "http://localhost"; // Replace with your Nakama server address
        [SerializeField] private int PORT = 7350; // Replace with your Nakama server port
        [SerializeField] private string SERVER_KEY = "defaultkey"; // Replace with your server key

        [Header("Debug")]
        [SerializeField] private bool _isHost;
        public bool IsHost { get => _isHost; private set => _isHost = value; }

        [SerializeField] private string _playerId;
        public string PlayerId { get => _playerId; private set => _playerId = value; }

        [SerializeField] private string _roomKey;
        public string RoomKey { get => _roomKey; private set => _roomKey = value; }

        private IClient client;
        private ISession session;
        private ISocket socket;
        private IMatch currentMatch;

        public static NakamaNetworkManager Instance { get; private set; }

        public bool IsRoomFull => totalPlayersJoinedRoom == MAX_PLAYERS;

        #region Unity Lifecycle Related
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitConnectionClient();
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

        public void InitConnectionClient()
        {
            try
            {
                client = new Client(SCHEME, HOST, PORT, SERVER_KEY);
                client.Timeout = 5;
                RegisterMatchPresencesListeners();
                RegisterMatchStateListeners();
            }
            catch (System.Exception e)
            {
                // ToastUtil.ShowErrorToast($"Failed to initialize Nakama client: {e.Message}");
                Debug.LogError($"Failed to initialize Nakama client: {e.Message}");
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

        public async Task Connect(string deviceId)
        {
            try
            {
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
            }
            catch (System.Exception e)
            {
                OnConnectionStateUpdated?.Invoke(ConnectionStatus.Failed);
                ToastUtil.ShowErrorToast($"Failed to connect: {e.Message}");
            }
        }
        #endregion

        #region Room Related
        public async Task CreateRoom()
        {
            var match = await socket.CreateMatchAsync();
            currentMatch = match;
            IsHost = true;
            RoomKey = match.Id;
            Debug.Log($"Created room with key: {RoomKey}");

            OnRoomCreated?.Invoke(PlayerId);
            totalPlayersJoinedRoom++;
        }


        public async Task JoinRoom(string roomKey)
        {
            try
            {
                var match = await socket.JoinMatchAsync(roomKey);
                currentMatch = match;
                IsHost = false;
                RoomKey = roomKey;
            }
            catch (System.Exception e)
            {
                ToastUtil.ShowErrorToast($"Failed to join room: {e.Message}");
            }
        }

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

        public async Task LeaveRoom()
        {
            await socket.LeaveMatchAsync(currentMatch.Id);
            currentMatch = null;
            IsHost = false;
            RoomKey = null;

            OnLeaveRoom?.Invoke();
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
                        NotifyClientNewPlayerJustJoined();
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
                else if (matchState.OpCode == NetworkConstants.LOBBY_PLAYER_JOINED_OP_CODE) // Player joined
                {
                    totalPlayersJoinedRoom++;
                    NotifyClientNewPlayerJustJoined();
                }
                else if (matchState.OpCode == NetworkConstants.LOBBY_GAME_START_OP_CODE) // Game start
                {
                    LoadGameplayScene("Carnival");
                }
                else if (matchState.OpCode == NetworkConstants.LOBBY_PLAYER_LOADED_OP_CODE) // Player loaded
                {
                    playersLoadedCarnivalCount++;
                    if (playersLoadedCarnivalCount == totalPlayersJoinedRoom)
                    {
                        OnAllPlayersLoaded?.Invoke();
                    }
                }
            };
        }

        private void NotifyClientNewPlayerJustJoined()
        {
            if (IsHost)
            {
                var readyState = new { TotalPlayers = totalPlayersJoinedRoom };
                string stateJson = JsonWriter.ToJson(readyState);
                var opCode = NetworkConstants.LOBBY_PLAYER_JOINED_OP_CODE; // Use opcode 3 for ready state updates
                socket.SendMatchStateAsync(currentMatch.Id, opCode, stateJson);
            }
        }

        public void SetPlayerReady(bool isReady)
        {
            if (currentMatch == null) return;

            var readyState = new { IsReady = isReady };
            string stateJson = JsonWriter.ToJson(readyState);
            var opCode = NetworkConstants.LOBBY_READY_OP_CODE; // Use opcode 3 for ready state updates
            socket.SendMatchStateAsync(currentMatch.Id, opCode, stateJson);
        }

        public void StartGame()
        {
            if (currentMatch == null || !IsHost) return;

            var opCode = NetworkConstants.LOBBY_GAME_START_OP_CODE; // Use opcode 4 for game start
            socket.SendMatchStateAsync(currentMatch.Id, opCode, "");
        }

        public void TryStartGame()
        {
            if (totalPlayersJoinedRoom > MAX_PLAYERS || totalPlayersJoinedRoom < 1) return;
            StartGame();
        }
        #endregion

        public void SendPlayerState(PlayerNetworkState playerState)
        {
            if (currentMatch == null) return;

            string stateJson = JsonWriter.ToJson(playerState);
            var opCode = NetworkConstants.GAMEPLAY_PLAYER_STATE_OP_CODE; // Use opcode 1 for player state updates
            socket.SendMatchStateAsync(currentMatch.Id, opCode, stateJson);
        }

        public void BroadcastWorldState(WorldNetworkState worldState)
        {
            if (currentMatch == null || !IsHost) return;

            string stateJson = JsonWriter.ToJson(worldState);
            var opCode = NetworkConstants.GAMEPLAY_WORLD_STATE_OP_CODE; // Use opcode 2 for world state updates
            socket.SendMatchStateAsync(currentMatch.Id, opCode, stateJson);
        }

        public void RegisterPlayerStateHandler(System.Action<PlayerNetworkState> onPlayerStateReceived)
        {
            socket.ReceivedMatchState += matchState =>
            {
                if (matchState.OpCode == NetworkConstants.GAMEPLAY_PLAYER_STATE_OP_CODE) // Check if it's a player state update
                {
                    var playerState = JsonParser.FromJson<PlayerNetworkState>(ConvertByteArrayToString(matchState.State));
                    onPlayerStateReceived(playerState);
                }
            };
        }

        public void UnregisterPlayerStateHandler(System.Action<PlayerNetworkState> onPlayerStateReceived)
        {
            socket.ReceivedMatchState -= matchState =>
            {
                if (matchState.OpCode == NetworkConstants.GAMEPLAY_PLAYER_STATE_OP_CODE) // Check if it's a player state update
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
                if (matchState.OpCode == NetworkConstants.GAMEPLAY_WORLD_STATE_OP_CODE) // Check if it's a world state update
                {
                    var worldState = JsonParser.FromJson<WorldNetworkState>(ConvertByteArrayToString(matchState.State));
                    onWorldStateReceived(worldState);
                }
            };
        }

        public void UnregisterWorldStateHandler(System.Action<WorldNetworkState> onWorldStateReceived)
        {
            socket.ReceivedMatchState -= matchState =>
            {
                if (matchState.OpCode == NetworkConstants.GAMEPLAY_WORLD_STATE_OP_CODE) // Check if it's a world state update
                {
                    var worldState = JsonParser.FromJson<WorldNetworkState>(ConvertByteArrayToString(matchState.State));
                    onWorldStateReceived(worldState);
                }
            };
        }

        #region Scene Related
        private void LoadGameplayScene(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName).completed += OnSceneLoaded;
        }

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
        private string ConvertByteArrayToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
        #endregion


    }
}
