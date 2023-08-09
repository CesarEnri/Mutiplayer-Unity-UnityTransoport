using System;
using System.Collections.Generic;
using Networking.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Networking.Server
{
    public class NetworkServer: IDisposable
    {
        private NetworkManager _networkManager;

        private Dictionary<ulong, string> _clientIdToAuth = new();
        private Dictionary<string, UserData> _authIdToUserData = new();
        
        
        public NetworkServer(NetworkManager networkManager)
        {
            _networkManager = networkManager;

            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += OnNetworkReady;
        }
        

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            var userData = JsonUtility.FromJson<UserData>(payload);

            _clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
            _authIdToUserData[userData.userAuthId] = userData;

            response.Approved = true;
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
                _authIdToUserData.Remove(authId);
            }
        }

        public void Dispose()
        {
            // TODO release managed resources here
            if(_networkManager == null)
                return;

            _networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            _networkManager.OnClientDisconnectCallback -= ApprovalCheck;
            _networkManager.OnServerStarted -= ApprovalCheck;
        }
    }
}