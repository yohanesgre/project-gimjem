using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GimJem.Network
{
    // Define your WorldState class to represent the game state
    [System.Serializable]
    public class WorldNetworkState
    {
        public List<PlayerNetworkState> Players { get; set; }
        public List<EnemyNetworkState> Enemies { get; set; }
        // Add other relevant game state properties
    }
}
