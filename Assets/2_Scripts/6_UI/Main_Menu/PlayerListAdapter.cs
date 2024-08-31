using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.MainMenu

{
    public class PlayerListAdapter : MonoBehaviour
    {
        [SerializeField] private GameObject playerItemPrefab;
        [SerializeField] private readonly Dictionary<string, PlayerListItem> playerItems = new();

        public void SetPlayerList(List<string> playersIds)
        {
            foreach (var playerId in playersIds)
            {
                if (playerItems.ContainsKey(playerId))
                {
                    continue;
                }

                AddPlayer(playerId);
            }
        }

        public void AddPlayer(string playerId)
        {
            if (playerItems.ContainsKey(playerId))
            {
                return;
            }

            var playerItem = Instantiate(playerItemPrefab, transform).GetComponent<PlayerListItem>();
            playerItems.Add(playerId, playerItem);
        }

        public void RemovePlayer(string playerId)
        {
            if (!playerItems.ContainsKey(playerId))
            {
                return;
            }

            Destroy(playerItems[playerId].gameObject);
        }

        public void SetPlayerReady(string playerId, bool isReady)
        {
            if (!playerItems.ContainsKey(playerId))
            {
                return;
            }

            playerItems[playerId].SetPlayerReady(isReady);
        }

        public void SetPlayerName(string playerId, string name)
        {
            if (!playerItems.ContainsKey(playerId))
            {
                return;
            }

            playerItems[playerId].SetPlayerName(name);
        }
    }
}