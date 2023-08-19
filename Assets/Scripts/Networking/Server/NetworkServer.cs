using System;
using System.Collections.Generic;
using Core;
using Networking.Shared;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Networking.Server
{
    public class NetworkServer: IDisposable
    {
        private NetworkManager _networkManager;

        public Action<string> OnClientLeft;
        public Action<GameData> OnUserJoined;
        public Action<GameData> OnUserLeft;
      

        private Dictionary<ulong, string> _clientIdToAuth = new();
        private Dictionary<string, GameData> _authIdToUserData = new();
        
        
        public NetworkServer(NetworkManager networkManager)
        {
            _networkManager = networkManager;

            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += OnNetworkReady;
        }


        public bool OpenConnection(string ip, int port)
        {
            var transport =_networkManager.gameObject.GetComponent<UnityTransport>();
            transport.SetConnectionData(ip, (ushort)port);
            return _networkManager.StartClient();
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            var userData = JsonUtility.FromJson<GameData>(payload);

            _clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
            _authIdToUserData[userData.userAuthId] = userData;
            OnUserJoined?.Invoke(userData);
            
            //Accion cuando el jugador entra a la partida
            response.Approved = true;
            response.Position = SpawnPoint.GetRandomSpawnPos();
            response.Rotation = Quaternion.identity;
            response.CreatePlayerObject = true;
        }
        
        private void OnNetworkReady()
        {
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (_clientIdToAuth.TryGetValue(clientId, out string authId))
            {
                _clientIdToAuth.Remove(clientId);
                OnUserLeft?.Invoke(_authIdToUserData[authId]);
                _authIdToUserData.Remove(authId);
                OnClientLeft?.Invoke(authId);
            }
        }

      

        public GameData GetUserDataByClientID(ulong clientId)
        {
            if (_clientIdToAuth.TryGetValue(clientId, out string authId))
            {
                if (_authIdToUserData.TryGetValue(authId, out GameData data))
                {
                    return data;
                }

                return null;
            }

            return null;
        }

        public void Dispose()
        {
            // TODO release managed resources here
            if(_networkManager == null)
                return;

            _networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            _networkManager.OnServerStarted -= OnNetworkReady;

            if (_networkManager.IsListening)
            {
                _networkManager.Shutdown();
            }
        }
    }
}
