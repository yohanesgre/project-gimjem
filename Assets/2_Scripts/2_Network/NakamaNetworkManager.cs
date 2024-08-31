using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GimJem.Core;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GimJem.Network
{
    public class NakamaNetworkManager : MonoBehaviour
    {
        public event System.Action<string> OnPlayerJoined;
        public event System.Action<string> OnPlayerLeft;
        public event System.Action<string, bool> OnPlayerReady;
        public event System.Action OnGameStarted;
        public event System.Action OnAllPlayersLoaded;

        private int playersLoadedCount = 0;
        private int totalPlayers = 0;

        private const string SCHEME = "http"; // Replace with your Nakama server address scheme
        private const string HOST = "http://localhost"; // Replace with your Nakama server address
        private const int PORT = 7350; // Replace with your Nakama server port
        private const string SERVER_KEY = "defaultkey"; // Replace with your server key

        private IClient client;
        private ISession session;
        private ISocket socket;
        private IMatch currentMatch;

        public bool IsHost { get; private set; }
        public string PlayerId { get; private set; }
        public string RoomKey { get; private set; }

        public static NakamaNetworkManager Instance { get; private set; }

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
        /// Connects to the Nakama server. This method is automatically called when Connect() is called.
        /// </summary>
        public void ConnectToServer()
        {
            client = new Client(SCHEME, HOST, PORT, SERVER_KEY)
            {
                Timeout = 5
            };
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
            session = await client.AuthenticateDeviceAsync(deviceId);
            Debug.Log("Connected to Nakama server");

            socket = client.NewSocket();
            await socket.ConnectAsync(session);
            Debug.Log("Connected to Nakama socket");

            PlayerId = session.UserId;
        }
        #endregion

        #region Room Related
        /// <summary>
        /// Creates a new room on the Nakama server.
        /// </summary>
        /// <returns>The room key created.</returns>
        public async Task<string> CreateRoom()
        {
            var match = await socket.CreateMatchAsync();
            currentMatch = match;
            IsHost = true;
            RoomKey = match.Id;
            Debug.Log($"Created room with key: {RoomKey}");

            RegisterMatchListeners();

            return RoomKey;
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

                RegisterMatchListeners();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to join room: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Registers listeners for match presence events (i.e. players joining or leaving).
        /// </summary>
        private void RegisterMatchListeners()
        {
            socket.ReceivedMatchPresence += match =>
            {
                foreach (var presence in match.Joins)
                {
                    OnPlayerJoined?.Invoke(presence.UserId);
                }
                foreach (var presence in match.Leaves)
                {
                    OnPlayerLeft?.Invoke(presence.UserId);
                }
            };
        }

        /// <summary>
        /// Registers listeners for match state events, specifically for handling player ready states and game start events.
        /// </summary>
        public void RegisterLobbyHandlers()
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
