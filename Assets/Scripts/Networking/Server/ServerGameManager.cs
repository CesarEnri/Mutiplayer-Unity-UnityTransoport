using System;
using System.Threading.Tasks;
using Networking.Server.Services;
using Unity.Netcode;
using Unity.Services.Multiplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Server
{
    public class ServerGameManager : IDisposable
    {
        private string _serverIP;
        private int _serverPort;
        private int _queryPort;

        private NetworkServer _networkServer;
        private MultiplayAllocationService _multiplayAllocationService;
        
        private const string GameSceneName = "Game";
        
        public ServerGameManager(string serverIp, int serverPort, int serverQPort,NetworkManager manager)
        {
            _serverIP = serverIp;
            _serverPort = serverPort;
            _queryPort = serverQPort;
            _networkServer = new NetworkServer(manager);
            _multiplayAllocationService = new MultiplayAllocationService();
        }

        public async Task StartGameServerAsync()
        {
            await _multiplayAllocationService.BeginServerCheck();

            if (!_networkServer.OpenConnection(_serverIP, _serverPort))
            {
                Debug.Log("NetworkServer did not start as expected.");
                return;
            }

            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }
        
        public void Dispose()
        {
            _multiplayAllocationService?.Dispose();
            _networkServer?.Dispose();
        }
    }
}