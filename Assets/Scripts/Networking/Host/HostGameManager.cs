using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Networking.Server;
using UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Host
{
    public class HostGameManager: IDisposable
    {
        private Allocation _allocation;
        private NetworkObject _playerPrefab;
        
        private const string GameSceneName = "Game";

        public string JoinCode { get; private set; }
        private string _lobbyId;

        public NetworkServer NetworkServer { get; private set; }

        private const int MaxConnections = 20;


        public HostGameManager(NetworkObject playerPrefab)
        {
            _playerPrefab = playerPrefab;
        }
        
        public async Task StartHostAsync(bool isPrivate)
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

            var relayServerData = new RelayServerData(_allocation, ApplicationController.ConfigProtocol);//dtls
            transport.SetRelayServerData(relayServerData);

            try
            {
                var lobbyOptions = new CreateLobbyOptions();
                lobbyOptions.IsPrivate = isPrivate;
                lobbyOptions.Data = new Dictionary<string, DataObject>()
                {
                    {
                        "JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Member,
                            value:JoinCode)
                    }
                };

                var nameLobbyPlayer = PlayerPrefs.GetString(NameSelected.PlayNameKey, "Unknown");
                var lobby = await Lobbies.Instance.CreateLobbyAsync(
                    $"{nameLobbyPlayer}'s Lobby ", MaxConnections, lobbyOptions);

                _lobbyId = lobby.Id;

                HostSingleton.Instance.StartCoroutine(HearBeatLobby(30));
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                return;
            }

            NetworkServer = new NetworkServer(NetworkManager.Singleton, _playerPrefab);

            var userData = new UserData 
            {
                userName = PlayerPrefs.GetString(NameSelected.PlayNameKey, "Missing Name"),
                userAuthId = AuthenticationService.Instance.PlayerId
            };

            var payload = JsonUtility.ToJson(userData);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            
            NetworkManager.Singleton.StartHost();
            NetworkServer.OnClientLeft += HandleClientLeft;
            
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }

        private IEnumerator HearBeatLobby(float waitTimeSeconds)
        {
            var delay =  new WaitForSecondsRealtime(waitTimeSeconds);
            
            while (true)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(_lobbyId);
                yield return delay;
            }
        }


        public void Dispose()
        {
            Shutdown();
        }

        public async Task Shutdown()
        {
            if (string.IsNullOrEmpty(_lobbyId))return;
            
                HostSingleton.Instance.StopCoroutine(nameof(HearBeatLobby));

                try
                {
                    await Lobbies.Instance.DeleteLobbyAsync(_lobbyId);
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }
                
                _lobbyId = string.Empty;
            
            NetworkServer.OnClientLeft -= HandleClientLeft;

            NetworkServer?.Dispose();
        }
        
        private async void HandleClientLeft(string authId)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_lobbyId, authId); 
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}