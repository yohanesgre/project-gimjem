using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GimJem.Network
{
    public class WorldNetworkStateManager : MonoBehaviour
    {
        [SerializeField] private float sendStateInterval = 0.1f;
        [SerializeField] private float interpolationTime = 0.1f;

        private WorldNetworkState currentWorldState;
        private WorldNetworkState previousWorldState;
        private float interpolationTimer;

        private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> enemies = new Dictionary<string, GameObject>();

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;

        private bool isHost;
        private string localPlayerId;

        public delegate void WorldStateUpdatedHandler(WorldNetworkState newState);
        public event WorldStateUpdatedHandler OnWorldStateUpdated;

        private void Awake()
        {
            currentWorldState = new WorldNetworkState
            {
                Players = new List<PlayerNetworkState>(),
                Enemies = new List<EnemyNetworkState>()
            };
            previousWorldState = new WorldNetworkState
            {
                Players = new List<PlayerNetworkState>(),
                Enemies = new List<EnemyNetworkState>()
            };
        }

        private void Start()
        {
            isHost = NakamaNetworkManager.Instance.IsHost;
            localPlayerId = NakamaNetworkManager.Instance.PlayerId;

            if (isHost)
            {
                NakamaNetworkManager.Instance.RegisterPlayerStateHandler(OnPlayerStateReceived);
                InvokeRepeating(nameof(BroadcastWorldState), 0f, sendStateInterval);
            }
            else
            {
                NakamaNetworkManager.Instance.RegisterWorldStateHandler(OnWorldStateReceived);
            }
        }

        private void Update()
        {
            if (!isHost)
            {
                InterpolateWorldState();
            }
        }

        public void UpdatePlayerState(string playerId, Vector3 position, Quaternion rotation)
        {
            if (isHost)
            {
                UpdateHostPlayerState(playerId, position, rotation);
            }
            else
            {
                SendPlayerState(playerId, position, rotation);
            }
        }

        private void UpdateHostPlayerState(string playerId, Vector3 position, Quaternion rotation)
        {
            var playerState = currentWorldState.Players.FirstOrDefault(p => p.Id == playerId);
            if (playerState != null)
            {
                playerState.Position = position;
                playerState.Rotation = rotation;
            }
            else
            {
                currentWorldState.Players.Add(new PlayerNetworkState { Id = playerId, Position = position, Rotation = rotation });
            }
        }

        private void SendPlayerState(string playerId, Vector3 position, Quaternion rotation)
        {
            PlayerNetworkState playerState = new PlayerNetworkState
            {
                Id = playerId,
                Position = position,
                Rotation = rotation
            };
            NakamaNetworkManager.Instance.SendPlayerState(playerState);
        }

        private void BroadcastWorldState()
        {
            if (!isHost) return;

            NakamaNetworkManager.Instance.BroadcastWorldState(currentWorldState);
        }

        private void OnPlayerStateReceived(PlayerNetworkState playerState)
        {
            if (!isHost) return;

            UpdateHostPlayerState(playerState.Id, playerState.Position, playerState.Rotation);
        }

        private void OnWorldStateReceived(WorldNetworkState worldState)
        {
            if (isHost) return;

            previousWorldState = currentWorldState;
            currentWorldState = worldState;
            interpolationTimer = 0f;

            OnWorldStateUpdated?.Invoke(currentWorldState);
        }

        private void InterpolateWorldState()
        {
            interpolationTimer += Time.deltaTime;
            float t = Mathf.Clamp01(interpolationTimer / interpolationTime);

            WorldNetworkState interpolatedState = new WorldNetworkState
            {
                Players = new List<PlayerNetworkState>(),
                Enemies = new List<EnemyNetworkState>()
            };

            foreach (var currentPlayer in currentWorldState.Players)
            {
                var previousPlayer = previousWorldState.Players.FirstOrDefault(p => p.Id == currentPlayer.Id);
                if (previousPlayer != null)
                {
                    interpolatedState.Players.Add(new PlayerNetworkState
                    {
                        Id = currentPlayer.Id,
                        Position = Vector3.Lerp(previousPlayer.Position, currentPlayer.Position, t),
                        Rotation = Quaternion.Slerp(previousPlayer.Rotation, currentPlayer.Rotation, t)
                    });
                }
                else
                {
                    interpolatedState.Players.Add(currentPlayer);
                }
            }

            // Similar interpolation for enemies...

            OnWorldStateUpdated?.Invoke(interpolatedState);
        }

        public PlayerNetworkState GetPlayerState(string playerId)
        {
            return currentWorldState.Players.FirstOrDefault(p => p.Id == playerId);
        }

        public List<PlayerNetworkState> GetAllPlayerStates()
        {
            return currentWorldState.Players;
        }

        // Add similar methods for enemies if needed
    }
}