using System;
using Core.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Leaderboard
{
    public class Leaderboard : NetworkBehaviour
    {
        [SerializeField] private Transform leaderboardEntityHolder;
        [SerializeField] private LeaderboardEntityDisplay leaderboardEntityDisplayPrefab;

        private NetworkList<LeaderboardEntityState> _leaderboardEntities;

        private void Awake()
        {
            _leaderboardEntities = new NetworkList<LeaderboardEntityState>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
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
                Coins = 0
            });
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
        }
        
        private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                    Instantiate(leaderboardEntityDisplayPrefab, leaderboardEntityHolder);                    
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Insert:
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.RemoveAt:
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Clear:
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Full:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
