using Networking.Client;
using Networking.Host;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace UI
{
    public class GameHUD : NetworkBehaviour
    {
        [SerializeField] private TMP_Text lobbyCodeText;
        
        [SerializeField] private GameObject androidControls;

        private NetworkVariable<FixedString32Bytes> lobbyCode = new("");
        
        private void Start()
        {
#if UNITY_EDITOR
            androidControls.SetActive(false);//false
#elif UNITY_ANDROID
            androidControls.SetActive(true);
#endif  
        }


        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                lobbyCode.OnValueChanged += HandleLobbyCodeChanged;
                HandleLobbyCodeChanged("", lobbyCode.Value);
                
            }

            if(!IsHost) return;
            
            lobbyCode.Value =  HostSingleton.Instance.HostGameManager.JoinCode;
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                lobbyCode.OnValueChanged -= HandleLobbyCodeChanged;
            }
            
        }
        
        private void HandleLobbyCodeChanged(FixedString32Bytes oldCode, FixedString32Bytes newCode)
        {
            lobbyCodeText.text = newCode.ToString();
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
