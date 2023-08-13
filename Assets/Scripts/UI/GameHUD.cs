using System;
using Networking.Client;
using Networking.Host;
using Unity.Netcode;
using UnityEngine;

namespace UI
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] private GameObject JoystickAndroid;
        
        private void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            JoystickAndroid.SetActive(true);
#endif  
        }

        public void LeaveGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HostSingleton.Instance.HostGameManager.Shutdown();
            }

            ClientSingleton.Instance.GameManager.Disconnect();
        }
    }
}
