using System.Collections;
using System.Collections.Generic;
using GimJem.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GimJem.UI.MainMenu
{
    public class DialogJoinRoomController : MonoBehaviour, IUIController<MainMenuManager>
    {
        private MainMenuManager manager;
        [SerializeField] private Button joinRoomButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_InputField roomKeyInputField;

        public void Init(MainMenuManager manager)
        {
            this.manager = manager;
        }

        private void Awake()
        {
            Hide();
        }

        private void OnEnable()
        {
            joinRoomButton.onClick.AddListener(OnClickJoinRoomButton);
            cancelButton.onClick.AddListener(OnClickCancelButton);
        }

        private void OnDisable()
        {
            joinRoomButton.onClick.RemoveListener(OnClickJoinRoomButton);
            cancelButton.onClick.RemoveListener(OnClickCancelButton);
        }

        private void OnClickJoinRoomButton()
        {
            // manager.JoinRoom(roomKeyInputField.text);
        }

        private void OnClickCancelButton()
        {
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
