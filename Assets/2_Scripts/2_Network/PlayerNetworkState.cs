using UnityEngine;

namespace GimJem.Network
{
    [System.Serializable]
    public class PlayerNetworkState
    {
        public string Id { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        // Add other player-specific properties
    }
}