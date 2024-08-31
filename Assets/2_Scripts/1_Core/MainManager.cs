
using GimJem.Network;
using UnityEngine;

namespace GimJem.Core
{
    public class MainManager : MonoBehaviour
    {
        public static MainManager Instance { get; private set; }
        public NakamaNetworkManager NetworkManager { get; private set; }
        [SerializeField] private NakamaNetworkManager nakamaNetworkManagerPrefab;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManagers();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeManagers()
        {
            NetworkManager = Instantiate(nakamaNetworkManagerPrefab);
            DontDestroyOnLoad(NetworkManager.gameObject);
            NetworkManager.ConnectToServer();
        }
    }
}