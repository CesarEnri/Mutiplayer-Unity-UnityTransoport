using System;
using System.Threading.Tasks;
using Networking.Server;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerGameManager : IDisposable
{
    private string serverIP;
    private int serverPort;
    private int queryPort;
    private MatchplayBackfiller backfiller;
#if UNITY_SERVER || UNITY_EDITOR      
    private MultiplayAllocationService multiplayAllocationService;
    #endif

    public NetworkServer NetworkServer { get; private set; }


    public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager manager, NetworkObject playerPrefab)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.queryPort = queryPort;
        NetworkServer = new NetworkServer(manager, playerPrefab);
#if UNITY_SERVER || UNITY_EDITOR     
        multiplayAllocationService = new MultiplayAllocationService();
#endif        
    }

    public async Task StartGameServerAsync()
    {
#if UNITY_SERVER || UNITY_EDITOR
        await multiplayAllocationService.BeginServerCheck();

        try
        {
            MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();

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

        if (!NetworkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogWarning("NetworkServer did not start as expected.");
            return;
        }

        #endif
    }

    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
#if UNITY_SERVER || UNITY_EDITOR
        Task<MatchmakingResults> matchmakerPayloadTask =
            multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        #endif
        return null;
    }

    private async Task StartBackfill(MatchmakingResults payload)
    {
        backfiller = new MatchplayBackfiller($"{serverIP}:{serverPort}",
            payload.QueueName,
            payload.MatchProperties,
            20);

        if (backfiller.NeedsPlayers())
        {
            await backfiller.BeginBackfilling();
        }
    }

    private void UserJoined(UserData user)
    {
#if UNITY_SERVER || UNITY_EDITOR
        backfiller.AddPlayerToMatch(user);
        multiplayAllocationService.AddPlayer();
        if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
        {
            _ = backfiller.StopBackfill();
        }
        #endif
    }

    private void UserLeft(UserData user)
    {
#if UNITY_SERVER || UNITY_EDITOR
        int playerCount = backfiller.RemovePlayerFromMatch(user.userAuthId);
        multiplayAllocationService.RemovePlayer();

        if (playerCount <= 0)
        {
            CloseServer();
            return;
        }

        if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
        {
            _ = backfiller.BeginBackfilling();
        }
        #endif
    }

    private async void CloseServer()
    {
        await backfiller.StopBackfill();
        Dispose();
        Application.Quit();
    }

    public void Dispose()
    {
#if UNITY_SERVER || UNITY_EDITOR
        NetworkServer.OnUserJoined -= UserJoined;
        NetworkServer.OnUserLeft -= UserLeft;

        backfiller?.Dispose();
        multiplayAllocationService?.Dispose();
        NetworkServer?.Dispose();
#endif        
    }
}
