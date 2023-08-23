using System;
using System.Text;
using System.Threading.Tasks;
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
        private MatchplayMatchmaker _matchplayMatchmaker;
        public UserData UserData { get; private set; }

        private const string MenuSceneName = "Menu";

        
        public async Task<bool> InitAsync()
        {
            await UnityServices.InitializeAsync();

            _networkClient = new NetworkClient(NetworkManager.Singleton);
            _matchplayMatchmaker = new MatchplayMatchmaker();

            var authState = await AuthenticationWrapper.DoAuth();

            if (authState == AuthState.Authenticated)
            {
                UserData = new UserData
                {
                    userName = PlayerPrefs.GetString(NameSelected.PlayNameKey, "Missing Name"),
                    userAuthId = AuthenticationService.Instance.PlayerId
                };

                return true;

            }

            return false;
        }

        public static void GoToMenu()
        {
            SceneManager.LoadScene(MenuSceneName);
        }

        public void StartClient(string ip, int port)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(ip, (ushort)port);
            ConnectClient();
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
            
            ConnectClient();
        }

        private void ConnectClient()
        {
            var payload = JsonUtility.ToJson(UserData);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            
            NetworkManager.Singleton.StartClient();
        }

        public async void MatchmakerAsync(GameInfo  gameInfo,Action<MatchmakerPollingResult> onMatchResponse)
        {
            if (_matchplayMatchmaker.IsMatchmaking)
            {
                return;
            }

            UserData.userGamePreferences.gameQueue = gameInfo.gameQueue;

            var matchResult  = await GetMatchAsync();
            onMatchResponse?.Invoke(matchResult);
        }

        private async Task<MatchmakerPollingResult> GetMatchAsync()
        {
            var matchmakingResult = await _matchplayMatchmaker.Matchmake(UserData);

            if (matchmakingResult.result == MatchmakerPollingResult.Success)
            {
                StartClient(matchmakingResult.ip, matchmakingResult.port);
            }

            return matchmakingResult.result;
        }

        public void Disconnect()
        {
            _networkClient.Disconnect();
        }
        
        public void Dispose()
        {
            _networkClient?.Dispose();
        }


        public async Task CancelMatchmaking()
        {
            await _matchplayMatchmaker.CancelMatchmaking();
        }
    }
}