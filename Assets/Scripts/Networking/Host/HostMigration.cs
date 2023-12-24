using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Networking.Host
{
    public class HostMigration : MonoBehaviour
    {
        /// <summary>
        /// It's value comes from either TryToQuickJoin or CreateMyRelayAndMyLobby
        /// </summary>
        private Lobby lobby;
        /// <summary>
        /// It's value comes from either CreateRelayAllocation
        /// </summary>
        private Allocation allocation;
        /// <summary>
        /// Comes from TryToJoinExistingLobbyOrCreateNewIfNoneFound
        /// </summary>
        private JoinAllocation joinAllocation;
        
        private string joinCode;
        public async void Start()
        {
            Debug.Log("??");
            await InitializeServicesAndSignInAsync();
            await TryToJoinExistingLobbyOrCreateNewIfNoneFound();
        }
        /// <summary>
        /// Is there a lobby that I can join using quick join? If there is such a lobby
        /// then join it, get it's allocation and join the relay. If not create the relay
        /// and create the lobby.
        /// </summary>
        /// <returns></returns>
        private async Task TryToJoinExistingLobbyOrCreateNewIfNoneFound()
        {
            try
            {
                await TryToQuickJoin();
                Debug.Log("If I arrived here I found an open lobby.");
                ///Maybe there is a issue here: when Alice creates a Lobby it sets an allocationId on herself.
                ///But when Bob gets the lobby created by Alice via quick join Alice's allocationId is gone.
                Debug.Log($"id:{lobby.Id}, hostId:{lobby.HostId}, numberOfPlayers:{lobby.Players.Count}, " +
                          $"allocationId:{lobby.Players.Where(p=>p.Id==lobby.HostId).First().AllocationId}");
                var connectionInfo = lobby.Players.Where(p => p.Id == lobby.HostId).First().ConnectionInfo;
                joinAllocation = await Relay.Instance.JoinAllocationAsync(connectionInfo);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
                {
                    Debug.Log("no lobby found, creating my own lobby");
                    await CreateMyRelayAndMyLobby();
                }
                else
                {
                    throw e;
                }
            }
        }
 
        private async Task InitializeServicesAndSignInAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("init done");
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }
        }
        /// <summary>
        /// Join an existing lobby. ***IS THIS CORRECT?***
        /// </summary>
        /// <returns></returns>
        private async Task TryToQuickJoin()
        {
            Debug.Log("Looking for existing lobby");
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
            options.Filter = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.MaxPlayers,
                    op: QueryFilter.OpOptions.GE,
                    value: "2"
                )
            };
            lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
        }
        /// <summary>
        /// Create the allocation and then the lobby using the allocation data.
        /// </summary>
        /// <returns></returns>
        private async Task CreateMyRelayAndMyLobby()
        {
            await CreateRelayAllocation();
            CreateLobbyOptions o = new CreateLobbyOptions();
            o.IsPrivate = false;
            o.Player = new Player(
                allocationId: allocation.AllocationId.ToString(),//****Alice's allocationId: is this correct ?****
                connectionInfo: joinCode,//****is this correct ?****
                id: AuthenticationService.Instance.PlayerId
            );
            lobby = await Lobbies.Instance.CreateLobbyAsync("foobar", 2, o);
            Debug.Log($"Created my lobby:{lobby.Id}, host:{lobby.HostId}, number of" +
                      $"players:{lobby.Players.Count}");
        }
        /// <summary>
        /// Creates the allocation, the values that matter go to the allocation and joinCode
        /// fields.
        /// </summary>
        /// <returns></returns>
        private async Task CreateRelayAllocation()
        {
            allocation = await Relay.Instance.CreateAllocationAsync(2);
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Created allocation:{allocation.AllocationId}");
        }
        private bool _createdHost;
        private bool _createdClient;
        public void Update()
        {
            var nm = NetworkManager.Singleton;
            if (allocation!=null && _createdHost == false)
            {
                _createdHost = true;
                nm.StartHost();
            }
            if(joinAllocation!=null && _createdClient == false)
            {
                _createdClient = true;
                nm.StartClient();
            }
        }
        
        /// <summary>
        /// Call event when host need migrate or leave the party.
        /// </summary>
        public void OnMigrateClick()
        {
            Debug.Log("clicou");
            doMigrate();
        }
        
        
        private async void doMigrate()
        {
            var updatedLobby = await Lobbies.Instance.GetLobbyAsync(lobby.Id);
            var nm = NetworkManager.Singleton;
            if (nm.IsHost)
            {
                Debug.Log("I am the host, have to migrate host to someone else");
                Lobby shouldHaveDifferentHost = await Lobbies.Instance.UpdateLobbyAsync(updatedLobby.Id, new UpdateLobbyOptions()
                {
                    HostId = updatedLobby.Players.Where(p => p.Id != updatedLobby.HostId).First().Id
                });
                Debug.Log($"Has the host id changed? {shouldHaveDifferentHost.HostId != updatedLobby.HostId}");
                await Lobbies.Instance.RemovePlayerAsync(updatedLobby.Id, updatedLobby.HostId);
                await Task.Delay(5000);//Waiting because host migration is not instantaneous
                Debug.Log("Waited for migration, time do disconnect from the game");
                nm.DisconnectClient(nm.LocalClientId);
                nm.Shutdown(true);
                Debug.Log("I am dead. Did the host migrate?");
            }
        }
    }
}
