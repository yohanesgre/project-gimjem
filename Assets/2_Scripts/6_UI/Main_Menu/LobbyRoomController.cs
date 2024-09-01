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
        [SerializeField] private Button readyButton;

        [SerializeField] private TMP_Text playerCountText;
        [SerializeField] private PlayerListAdapter playerListAdapter;
        [SerializeField] private int playerCount = 0;
        [SerializeField] private bool isHost = false;
        [SerializeField] private bool isReady = false;

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
            manager.OnLeaveRoom += OnLeaveRoom;
        }

        private void OnEnable()
        {
            startGameButton.onClick.AddListener(OnClickStartGameButton);
            leaveRoomButton.onClick.AddListener(OnClickLeaveRoomButton);
            readyButton.onClick.AddListener(OnClickReadyButton);
        }

        private void OnDisable()
        {
            startGameButton.onClick.RemoveListener(OnClickStartGameButton);
            leaveRoomButton.onClick.RemoveListener(OnClickLeaveRoomButton);
            readyButton.onClick.RemoveListener(OnClickReadyButton);
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.OnPlayerJoined -= OnPlayerJoined;
                manager.OnPlayerLeft -= OnPlayerLeft;
                manager.OnPlayerReadyChanged -= OnPlayerReady;
                manager.OnRoomCreated -= OnCreateRoom;
                manager.OnLeaveRoom -= OnLeaveRoom;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void OnClickStartGameButton()
        {
            manager.StartGame();
        }


        private void OnClickLeaveRoomButton()
        {
            manager.LeaveRoom();
        }

        private void OnClickReadyButton()
        {
            isReady = !isReady;
            manager.SetPlayerReady(isReady);
            readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Cancel" : "Ready";
            readyButton.GetComponent<Image>().color = isReady ? Color.red : Color.green;
        }

        private void OnCreateRoom(string playerId)
        {
            Show();
            AddPlayerItem(playerId, true, true, 1);
        }

        private void OnLeaveRoom()
        {
            Hide();
        }


        private void OnPlayerJoined(string playerId, bool isSelf, bool isHost, int minPlayerCount)
        {
            if (!isHost)
            {
                Show();
            }
            AddPlayerItem(playerId, isSelf, isHost, minPlayerCount);
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
            Debug.Log($"Player {playerId} is ready: {isReady}");
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

        private void AddPlayerItem(string playerId, bool isSelf, bool isHost, int minPlayerCount)
        {
            playerCount++;
            UpdatePlayerCountText(playerCount);
            playerListAdapter.AddPlayer(playerId);
            playerListAdapter.SetPlayerName(playerId, playerId);
            playerListAdapter.SetPlayerReady(playerId, false);
            UpdateStartGameButton(isHost, minPlayerCount);
        }
    }
}