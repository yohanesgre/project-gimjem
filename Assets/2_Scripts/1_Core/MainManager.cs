
using System;
using System.Threading.Tasks;
using GimJem.Network;
using UnityEngine;

namespace GimJem.Core
{
    public class MainManager : MonoBehaviour
    {
        public static MainManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

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

        private async Task TryConnectToServerAsync()
        {
            var deviceId = NakamaNetworkManager.Instance.GetDeviceId();
            await NakamaNetworkManager.Instance.Connect(deviceId);
        }

    }
}