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
#if UNITY_EDITOR_64 || UNITY_EDITOR || UNITY_EDITOR_WIN
            JoystickAndroid.SetActive(false);
#elif UNITY_ANDROID
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
