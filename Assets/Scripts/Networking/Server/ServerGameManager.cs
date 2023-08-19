
using System;
using System.Threading.Tasks;
using Networking.Server.Services;
using Networking.Shared;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Server
{
    public class ServerGameManager : IDisposable
    {
        private string _serverIP;
        private int _serverPort;
        private int _queryPort;

        private MatchplayBackfiller _matchplayBackfiller;


#if UNITY_SERVER || UNITY_EDITOR
        private MultiplayAllocationService _multiplayAllocationService;
#endif
        
        public NetworkServer NetworkServer { get; private set; }
        
        private const string GameSceneName = "Game";
        
        public ServerGameManager(string serverIp, int serverPort, int serverQPort,NetworkManager manager)
        {
            _serverIP = serverIp;
            _serverPort = serverPort;
            _queryPort = serverQPort;
            NetworkServer = new NetworkServer(manager);
#if UNITY_SERVER || UNITY_EDITOR
            _multiplayAllocationService = new MultiplayAllocationService();
#endif            
        }

        public async Task StartGameServerAsync()
        {
#if UNITY_SERVER || UNITY_EDITOR
            await _multiplayAllocationService.BeginServerCheck();

            try
            {
                var matchmakerPayload = await GetMatchmakerPayload();

                if (matchmakerPayload != null)
                {
                    await StartBackfill(matchmakerPayload);
                    NetworkServer.OnUserJoined += UserJoined;
                    NetworkServer.OnUserLeft += UserLeft;
                }
                else
                {
                    Debug.LogWarning("Matchmaker payload timed out");
                }
            }
            catch (Exception e)
            {
             Debug.LogWarning(e);
            }
            

            if (!NetworkServer.OpenConnection(_serverIP, _serverPort))
            {
                Debug.Log("NetworkServer did not start as expected.");
                return;
            }

            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
#endif            
        }

        private async Task StartBackfill(MatchmakingResults matchmakerPayload)
        {
            _matchplayBackfiller = new MatchplayBackfiller($"{_serverIP}: {_serverPort}", 
                matchmakerPayload.QueueName, matchmakerPayload.MatchProperties, 20);

            if (_matchplayBackfiller.NeedsPlayers())
            {
                await _matchplayBackfiller.BeginBackfilling();                
            }
        }


        private void UserJoined(GameData user)
        {
#if UNITY_SERVER || UNITY_EDITOR
            _matchplayBackfiller.AddPlayerToMatch(user);
            _multiplayAllocationService.AddPlayer();
            if (!_matchplayBackfiller.NeedsPlayers() && _matchplayBackfiller.IsBackfilling)
            {
                _ = _matchplayBackfiller.StopBackfill();
                
            }
#endif            
        }

        private void UserLeft(GameData user)
        {
#if UNITY_SERVER || UNITY_EDITOR
            var playerCount = _matchplayBackfiller.RemovePlayerFromMatch(user.userAuthId);
            _multiplayAllocationService.RemovePlayer();

            if (playerCount <= 0)
            {
                CloseServer();
                return;
            }

            if (_matchplayBackfiller.NeedsPlayers() && !_matchplayBackfiller.IsBackfilling)
            {
                _ = _matchplayBackfiller.BeginBackfilling();
            }
            #endif
        }

        private async void CloseServer()
        {
            await _matchplayBackfiller.StopBackfill();
            Dispose();
            Application.Quit();
        }

        private async Task<MatchmakingResults> GetMatchmakerPayload()
        {
#if UNITY_SERVER || UNITY_EDITOR
            var matchMakerPayloadTask = _multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

            if (await Task.WhenAny(matchMakerPayloadTask, Task.Delay(20000)) == matchMakerPayloadTask)
            {
                return matchMakerPayloadTask.Result;
            }

            #endif
            return null;
        }

        public void Dispose()
        {
#if UNITY_SERVER || UNITY_EDITOR
            NetworkServer.OnUserJoined -= UserJoined;
            NetworkServer.OnUserLeft -= UserLeft;
            
            _matchplayBackfiller?.Dispose();
            _multiplayAllocationService?.Dispose();
            NetworkServer?.Dispose();
#endif            
        }
    }
}
