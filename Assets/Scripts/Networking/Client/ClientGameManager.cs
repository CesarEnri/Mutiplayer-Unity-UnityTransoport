using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Client
{
    public class ClientGameManager
    {
        private JoinAllocation _allocation;

        private const string MenuSceneName = "Menu";

        public async Task<bool> InitAsync()
        {
            await UnityServices.InitializeAsync();

            var authState = await AuthenticationWrapper.DoAuth();

            return authState == AuthState.Authenticated;
        }

        public static void GoToMenu()
        {
            SceneManager.LoadScene(MenuSceneName);
        }

        public async Task StartClientAsync(string joinCode)
        {
            try
            {
                _allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            var relayServerData = new RelayServerData(_allocation, "udp");//dtls
            transport.SetRelayServerData(relayServerData);
            
            NetworkManager.Singleton.StartClient();
        }
    }
}