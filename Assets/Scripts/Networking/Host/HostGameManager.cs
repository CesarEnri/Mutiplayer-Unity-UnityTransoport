using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Networking.Server;
using Networking.Shared;
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
    public class HostGameManager
    {
        private Allocation _allocation;
        private const string GameSceneName = "Game";

        private string _joinCode;
        private string _lobbyId;

        private NetworkServer _networkServer;
        
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
                _joinCode = await Relay.Instance.GetJoinCodeAsync(_allocation.AllocationId);
                Debug.Log(_joinCode);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            var relayServerData = new RelayServerData(_allocation, "udp");//dtls
            transport.SetRelayServerData(relayServerData);

            try
            {
                var lobbyOptions = new CreateLobbyOptions();
                lobbyOptions.IsPrivate = false;
                lobbyOptions.Data = new Dictionary<string, DataObject>()
                {
                    {
                        "JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Member,
                            value:_joinCode)
                    }
                };

                var nameLobbyPlayer = PlayerPrefs.GetString(NameSelected.PlayNameKey, "Unknown");
                var lobby = await Lobbies.Instance.CreateLobbyAsync(
                    $"{nameLobbyPlayer}'s Lobby ", MaxConnections, lobbyOptions);

                _lobbyId = lobby.Id;

                HostSingleton.Instance.StartCoroutine(HearBeatLobby(15));
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                return;
            }

            _networkServer = new NetworkServer(NetworkManager.Singleton);

            var userData = new UserData
            {
                userName = PlayerPrefs.GetString(NameSelected.PlayNameKey, "Missing Name"),
                userAuthId = AuthenticationService.Instance.PlayerId
            };

            var payload = JsonUtility.ToJson(userData);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            
            NetworkManager.Singleton.StartHost();
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


    }
}