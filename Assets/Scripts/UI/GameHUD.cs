using System.Threading.Tasks;
using Networking.Client;
using Networking.Host;
using Unity.Netcode;
using UnityEngine;

namespace UI
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] private GameObject joystickAndroid;
        
        private void Start()
        {
#if UNITY_EDITOR
            joystickAndroid.SetActive(false);
#elif UNITY_ANDROID
            joystickAndroid.SetActive(true);
#endif  
        }

        public void  LeaveGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                HostSingleton.Instance.HostGameManager.Shutdown();
            }

            ClientSingleton.Instance.GameManager.Disconnect();
        }
    }
}
