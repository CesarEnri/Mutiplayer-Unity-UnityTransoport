using System;
using System.Collections.Generic;
using System.Linq;
using Core.Coins;
using Core.Player;
using Networking.Client;
using Unity.Netcode;
using UnityEngine;

namespace UI.Leaderboard
{
    public class Leaderboard : NetworkBehaviour
    {
        [SerializeField] private Transform leaderboardEntityHolder;
        [SerializeField] private Transform teamLeaderboardEntityHolder;
        [SerializeField] private GameObject teamLeaderBoardBackground;
        
        [SerializeField] private LeaderboardEntityDisplay leaderboardEntityDisplayPrefab;
        [SerializeField] private int entitiesToDisplay = 8;
        [SerializeField] private Color ownerColour;
        [SerializeField] private string[] teamNames;
        [SerializeField] private TeamColourLookup teamColourLookup;

        private NetworkList<LeaderboardEntityState> _leaderboardEntities;
       
        private List<LeaderboardEntityDisplay> _entityDisplays = new();
        private List<LeaderboardEntityDisplay> teamEntityDisplays = new();

        private void Awake()
        {
            _leaderboardEntities = new NetworkList<LeaderboardEntityState>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                if (ClientSingleton.Instance.GameManager.UserData.userGamePreferences.gameQueue == GameQueue.Team)
                {
                    teamLeaderBoardBackground.SetActive(true);

                    for (var i = 0; i < teamNames.Length; i++)
                    {
                        var teamLeaderBoardEntity = Instantiate(leaderboardEntityDisplayPrefab, teamLeaderboardEntityHolder);
                        
                        teamLeaderBoardEntity.Initialise(i, teamNames[i], 0);

                        var teamColour = teamColourLookup.GetTeamColour(i);
                        teamLeaderBoardEntity.SetColour(teamColour);
                        
                        teamEntityDisplays.Add(teamLeaderBoardEntity);
                    }
                    
                }

                _leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
                foreach (var entity in _leaderboardEntities)
                {
                    HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState>
                    {
                        Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                        Value = entity
                    });                    
                }
            }

            
            if (IsServer)
            {
                TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
                foreach (TankPlayer player in players)
                {
                    HandlePlayerSpawned(player);
                }

                TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
                TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                _leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
            }

            if (IsServer)
            {
                TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
                TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
            }
        }

        private void HandlePlayerSpawned(TankPlayer player)
        {
            _leaderboardEntities.Add(new LeaderboardEntityState
            {
                ClientId = player.OwnerClientId,
                PlayerName = player.PlayerName.Value,
                TeamIndex = player.TeamIndex.Value,
                Coins = 0
            });

            player.Wallet.totalCoins.OnValueChanged += (oldCoins, newCoins) => HandleCoinsChange(player.OwnerClientId, newCoins);
        }
        
        private void HandlePlayerDespawned(TankPlayer player)
        {
            if(_leaderboardEntities == null) return;
            
            foreach (var entity in _leaderboardEntities)
            {
                if (entity.ClientId != player.OwnerClientId) continue;

                _leaderboardEntities.Remove(entity);
                break;
            }
            
            player.Wallet.totalCoins.OnValueChanged -= (oldCoins, newCoins) => HandleCoinsChange(player.OwnerClientId, newCoins);
        }
        
        private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
        {
            if(!gameObject.scene.isLoaded) return;
            
            switch (changeEvent.Type)
            {
                case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                    if (!_entityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                    {
                        var leaderboardEntity = Instantiate(leaderboardEntityDisplayPrefab, leaderboardEntityHolder);
                        leaderboardEntity.Initialise(changeEvent.Value.ClientId, changeEvent.Value.PlayerName, changeEvent.Value.Coins);
                        if (NetworkManager.Singleton.LocalClientId == changeEvent.Value.ClientId)
                        {
                            leaderboardEntity.SetColour(ownerColour);
                        }

                        _entityDisplays.Add(leaderboardEntity);
                    }
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Insert:
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                    var displayToRemove  =_entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                    if (displayToRemove != null)
                    {
                         displayToRemove.transform.SetParent(null);
                         Destroy(displayToRemove.gameObject);
                         _entityDisplays.Remove(displayToRemove);
                    }
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.RemoveAt:
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                    var displayToUpdate  =_entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                    if (displayToUpdate != null)
                    {
                        displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                    }
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Clear:
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Full:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));

            for (var i = 0; i < _entityDisplays.Count; i++)
            {
                _entityDisplays[i].transform.SetSiblingIndex(i);
                _entityDisplays[i].UpdateText();
                _entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
            }

            var myDisplay = _entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

            if (myDisplay != null)
            {
                if (myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
                {
                    leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                    myDisplay.gameObject.SetActive(true);
                }
            }
            
            if(!teamLeaderBoardBackground.activeSelf) return;
            
            var teamDisplay = teamEntityDisplays.FirstOrDefault(x => x.TeamIndex == changeEvent.Value.TeamIndex);

            if (teamDisplay != null)
            {
                if (changeEvent.Type == NetworkListEvent<LeaderboardEntityState>.EventType.Remove)
                {
                    teamDisplay.UpdateCoins(teamDisplay.Coins - changeEvent.Value.Coins);
                }
                else
                {
                    teamDisplay.UpdateCoins(teamDisplay.Coins + (changeEvent.Value.Coins - changeEvent.PreviousValue.Coins));
                }

                teamEntityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));

                for (int i = 0; i < teamEntityDisplays.Count; i++)
                {
                    teamEntityDisplays[i].transform.SetSiblingIndex(i);
                    teamEntityDisplays[i].UpdateText();
                }
            }

        }
         private void HandleCoinsChange(ulong clientId, int newCoins)
        {
            for (var i = 0; i < _leaderboardEntities.Count; i++)
            {
                if (_leaderboardEntities[i].ClientId != clientId)
                {
                    continue;
                }

                _leaderboardEntities[i] = new LeaderboardEntityState
                {
                    ClientId = _leaderboardEntities[i].ClientId,
                    PlayerName = _leaderboardEntities[i].PlayerName,
                    TeamIndex = _leaderboardEntities[i].TeamIndex,
                    Coins = newCoins
                };
            }  
        }
    }
}
