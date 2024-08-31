using System.ComponentModel;
using GimJem.Core;
using GimJem.Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GimJem.UI.MainMenu
{
    public class MainMenuController : MonoBehaviour, IUIController<MainMenuManager>
    {
        private MainMenuManager manager;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button connectButton;
        [SerializeField] private Button playOfflineButton;


        public void Init(MainMenuManager manager)
        {

            this.manager = manager;
            manager.OnConnectionStateUpdated += OnConnectionStateUpdated;
        }

        private void Awake()
        {
            // default state
            connectButton.gameObject.SetActive(false);
            SetInteractableCreateRoomButton(false);
            SetInteractableJoinRoomButton(false);
        }

        private void OnDestroy()
        {
            manager.OnConnectionStateUpdated -= OnConnectionStateUpdated;

        }

        private void OnEnable()
        {
            createRoomButton.onClick.AddListener(OnClickCreateRoomButton);
            joinRoomButton.onClick.AddListener(OnClickJoinRoomButton);
            connectButton.onClick.AddListener(OnClickConnectButton);
            playOfflineButton.onClick.AddListener(OnClickPlayOfflineButton);
        }

        private void OnDisable()
        {
            createRoomButton.onClick.RemoveListener(OnClickCreateRoomButton);
            joinRoomButton.onClick.RemoveListener(OnClickJoinRoomButton);
            connectButton.onClick.RemoveListener(OnClickConnectButton);
            playOfflineButton.onClick.RemoveListener(OnClickPlayOfflineButton);
        }

        private void OnConnectionStateUpdated(ConnectionStatus status)
        {
            Debug.Log($"{GetType()} | Connection State Updated: {status}");
            switch (status)
            {
                case ConnectionStatus.Connected:
                    connectButton.gameObject.SetActive(false);
                    SetInteractableCreateRoomButton(true);
                    SetInteractableJoinRoomButton(true);
                    break;
                case ConnectionStatus.Disconnected:
                    connectButton.gameObject.SetActive(true);
                    SetInteractableCreateRoomButton(false);
                    SetInteractableJoinRoomButton(false);
                    break;
            }
        }


        private void OnClickCreateRoomButton()
        {
            manager.CreateRoomAsync();
        }

        private void OnClickJoinRoomButton()
        {
            manager.OpenJoinRoomDialog();
        }

        private void OnClickConnectButton()
        {
            Debug.Log("Connect");
        }

        private void OnClickPlayOfflineButton()
        {
            SceneManager.LoadScene("Carnival");
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void SetInteractableCreateRoomButton(bool enabled)
        {
            createRoomButton.interactable = enabled;
        }

        private void SetInteractableJoinRoomButton(bool enabled)
        {
            joinRoomButton.interactable = enabled;
        }
    }
}