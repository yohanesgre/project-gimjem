using GimJem.Core;

using UnityEngine;
using UnityEngine.UI;

namespace GimJem.UI.MainMenu
{
    public class MainMenuController : MonoBehaviour, IController<MainMenuManager>
    {
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button joinRoomButton;

        public void Init(MainMenuManager manager)
        {

        }

        private void OnEnable()
        {
            createRoomButton.onClick.AddListener(OnClickCreateRoomButton);
            joinRoomButton.onClick.AddListener(OnClickJoinRoomButton);
        }

        private void OnDisable()
        {
            createRoomButton.onClick.RemoveListener(OnClickCreateRoomButton);
            joinRoomButton.onClick.RemoveListener(OnClickJoinRoomButton);
        }

        private void OnClickCreateRoomButton()
        {
            Debug.Log("Create Room");
        }

        private void OnClickJoinRoomButton()
        {
            Debug.Log("Join Room");
        }


    }
}