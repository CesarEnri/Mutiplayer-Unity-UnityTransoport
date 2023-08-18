using System;
using System.Text;
using System.Threading.Tasks;
using Networking.Shared;
using UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Client
{
    public class ClientGameManager : IDisposable
    {
        private JoinAllocation _allocation;
        private NetworkClient _networkClient;

        private const string MenuSceneName = "Menu";

        
        public async Task<bool> InitAsync()
        {
            await UnityServices.InitializeAsync();

            _networkClient = new NetworkClient(NetworkManager.Singleton);

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

            var relayServerData = new RelayServerData(_allocation, ApplicationController.ConfigProtocol);//dtls
            transport.SetRelayServerData(relayServerData);

            var userData = new GameData
            {
                userName = PlayerPrefs.GetString(NameSelected.PlayNameKey, "Missing Name"),
                userAuthId = AuthenticationService.Instance.PlayerId
            };

            var payload = JsonUtility.ToJson(userData);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            
            NetworkManager.Singleton.StartClient();
        }

        public void Disconnect()
        {
            _networkClient.Disconnect();
        }
        
        public void Dispose()
        {
            _networkClient?.Dispose();
        }

        
    }
}