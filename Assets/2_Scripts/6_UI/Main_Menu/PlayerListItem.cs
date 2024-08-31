using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.UI.MainMenu
{
    public class PlayerListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text playerReadyText;

        public void SetPlayerName(string name)
        {
            playerNameText.text = name;
        }

        public void SetPlayerReady(bool isReady)
        {
            playerReadyText.text = isReady ? "Ready" : "Not Ready";
            playerReadyText.color = isReady ? Color.green : Color.red;
        }
    }
}