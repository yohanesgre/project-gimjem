using GimJem.Core;
using GimJem.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.MainMenu
{
    public class LobbyRoomController : MonoBehaviour, IUIController<MainMenuManager>
    {
        private MainMenuManager manager;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button leaveRoomButton;
        [SerializeField] private TMP_Text playerCountText;
        [SerializeField] private PlayerListAdapter playerListAdapter;
        [SerializeField] private int playerCount = 1;
        [SerializeField] private bool isHost = false;

        private void Awake()
        {
            Hide();
        }

        public void Init(MainMenuManager manager)
        {
            this.manager = manager;
            manager.OnPlayerJoined += OnPlayerJoined;
            manager.OnPlayerLeft += OnPlayerLeft;
            manager.OnPlayerReadyChanged += OnPlayerReady;
            manager.OnRoomCreated += OnCreateRoom;
        }

        private void OnEnable()
        {
            startGameButton.onClick.AddListener(OnClickStartGameButton);
            leaveRoomButton.onClick.AddListener(OnClickLeaveRoomButton);
        }

        private void OnDisable()
        {
            startGameButton.onClick.RemoveListener(OnClickStartGameButton);
            leaveRoomButton.onClick.RemoveListener(OnClickLeaveRoomButton);
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.OnPlayerJoined -= OnPlayerJoined;
                manager.OnPlayerLeft -= OnPlayerLeft;
                manager.OnPlayerReadyChanged -= OnPlayerReady;
                manager.OnRoomCreated -= OnCreateRoom;
            }
        }

        public void Hide()
        {
            GetComponent<Canvas>().enabled = false;
        }

        public void Show()
        {
            GetComponent<Canvas>().enabled = true;
            // Debug.Log("OnCreateRoom");
        }

        private void OnClickStartGameButton()
        {
            manager.StartGame();
        }

        private void OnClickLeaveRoomButton()
        {
            manager.LeaveRoom();
        }

        private void OnCreateRoom()
        {
            Show();
        }

        private void OnPlayerJoined(string playerId, bool isSelf, bool isHost, int minPlayerCount)
        {
            playerCount++;
            UpdatePlayerCountText(playerCount);
            playerListAdapter.AddPlayer(playerId);
            playerListAdapter.SetPlayerName(playerId, playerId);
            playerListAdapter.SetPlayerReady(playerId, false);
            UpdateStartGameButton(isHost, minPlayerCount);
        }

        private void OnPlayerLeft(string playerId, bool isHost, int minPlayerCount)
        {
            playerCount--;
            UpdatePlayerCountText(playerCount);
            playerListAdapter.RemovePlayer(playerId);
            UpdateStartGameButton(isHost, minPlayerCount);
        }

        private void OnPlayerReady(string playerId, bool isReady, bool isHost)
        {
            playerListAdapter.SetPlayerReady(playerId, isReady);
        }

        private void UpdatePlayerCountText(int playerCount)
        {
            playerCountText.text = $"Total Players: {playerCount}";
        }

        private void UpdateStartGameButton(bool isHost, int minPlayerCount)
        {
            startGameButton.interactable = isHost && playerCount >= minPlayerCount;
        }
    }
}