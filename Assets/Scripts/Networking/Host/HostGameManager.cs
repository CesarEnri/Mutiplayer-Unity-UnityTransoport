using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Host
{
    public class HostGameManager
    {
        private Allocation _allocation;
        public const string GameSceneName = "Game";
        
        public string JoinCode;
        
        private const int MaxConnections = 20;
        
        public async Task StartHostAsync()
        {
            try
            {
                _allocation =  await Relay.Instance.CreateAllocationAsync(MaxConnections);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }
            
            
            try
            {
                JoinCode = await Relay.Instance.GetJoinCodeAsync(_allocation.AllocationId);
                Debug.Log(JoinCode);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            var relayServerData = new RelayServerData(_allocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }

        
    }
}