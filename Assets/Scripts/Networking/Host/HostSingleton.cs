using Unity.Netcode;
using UnityEngine;

namespace Networking.Host
{
    public class HostSingleton: MonoBehaviour
    {
        private static HostSingleton instance;
        public HostGameManager HostGameManager { get; private set; }

        public GameQueue gameQueue;
        public static HostSingleton Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = FindObjectOfType<HostSingleton>();

                if (instance == null)
                {
                    return null;
                }

                return instance;
            }
        }
        

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void CreateHost(NetworkObject playerPrefab)
        {
            HostGameManager = new HostGameManager(playerPrefab);
        }

        private void OnDestroy()
        {
            HostGameManager?.Dispose();
        }
    }
}