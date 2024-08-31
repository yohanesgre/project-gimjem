using System;
using System.Threading.Tasks;
using GimJem.Network;
using UnityEngine;

namespace GimJem.Core
{
    public class MainManager : MonoBehaviour
    {
        public static MainManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject nakamaNetworkManagerPrefab;

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

        private async void Start()
        {
            await TryConnectToServerAsync();
        }

        private void InitializeManagers()
        {
            InitializeNakamaNetworkManager();
        }

        private void InitializeNakamaNetworkManager()
        {
            Instantiate(nakamaNetworkManagerPrefab, transform.parent);
            NakamaNetworkManager.Instance.InitConnectionClient();
        }

        private async Task TryConnectToServerAsync()
        {
            var deviceId = NakamaNetworkManager.Instance.GetDeviceId();
            await NakamaNetworkManager.Instance.Connect(deviceId);
        }

        private void OnValidate()
        {
            if (nakamaNetworkManagerPrefab == null)
            {
                throw new System.Exception("NakamaNetworkManagerPrefab is not set");
            }
        }
    }
}