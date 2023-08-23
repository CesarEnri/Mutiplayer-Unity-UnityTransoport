using System.Collections.Generic;
using System.Linq;
using Core.Player;
using Networking.Client;
using Unity.Netcode;
using UnityEngine;

namespace UI.Leaderboard
{
    public class LeaderboardCoin : NetworkBehaviour
    {
        [SerializeField] private Transform leaderboardEntityHolder;
        [SerializeField] private Transform teamLeaderboardEntityHolder;
        [SerializeField] private GameObject teamLeaderboardBackground;
        [SerializeField] private LeaderboardEntityDisplayCoin leaderboardEntityPrefab;
        [SerializeField] private int entitiesToDisplay = 8;
        [SerializeField] private Color ownerColour;
        [SerializeField] private string[] teamNames;
        [SerializeField] private TeamColourLookup teamColourLookup;

        private NetworkList<LeaderboardEntityStateCoin> leaderboardEntities;
        private List<LeaderboardEntityDisplayCoin> entityDisplays = new();
        private List<LeaderboardEntityDisplayCoin> teamEntityDisplays = new();

        private void Awake()
        {
            leaderboardEntities = new NetworkList<LeaderboardEntityStateCoin>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                if (ClientSingleton.Instance.GameManager.UserData.userGamePreferences.gameQueue
                    == GameQueue.Team)
                {
                    teamLeaderboardBackground.SetActive(true);

                    for (int i = 0; i < teamNames.Length; i++)
                    {
                        LeaderboardEntityDisplayCoin teamLeaderboardEntity =
                            Instantiate(leaderboardEntityPrefab, teamLeaderboardEntityHolder);

                        teamLeaderboardEntity.Initialise(i, teamNames[i], 0);

                        Color teamColour = teamColourLookup.GetTeamColour(i);
                        teamLeaderboardEntity.SetColour(teamColour);

                        teamEntityDisplays.Add(teamLeaderboardEntity);
                    }
                }

                leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
                foreach (LeaderboardEntityStateCoin entity in leaderboardEntities)
                {
                    HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityStateCoin>
                    {
                        Type = NetworkListEvent<LeaderboardEntityStateCoin>.EventType.Add,
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
                leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
            }

            if (IsServer)
            {
                TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
                TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
            }
        }

        private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityStateCoin> changeEvent)
        {
            if (!gameObject.scene.isLoaded) { return; }

            switch (changeEvent.Type)
            {
                case NetworkListEvent<LeaderboardEntityStateCoin>.EventType.Add:
                    if (!entityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                    {
                        LeaderboardEntityDisplayCoin leaderboardEntity =
                            Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                        leaderboardEntity.Initialise(
                            changeEvent.Value.ClientId,
                            changeEvent.Value.PlayerName,
                            changeEvent.Value.Coins);
                        if (NetworkManager.Singleton.LocalClientId == changeEvent.Value.ClientId)
                        {
                            leaderboardEntity.SetColour(ownerColour);
                        }
                        entityDisplays.Add(leaderboardEntity);
                    }
                    break;
                case NetworkListEvent<LeaderboardEntityStateCoin>.EventType.Remove:
                    LeaderboardEntityDisplayCoin displayCoinToRemove =
                        entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                    if (displayCoinToRemove != null)
                    {
                        displayCoinToRemove.transform.SetParent(null);
                        Destroy(displayCoinToRemove.gameObject);
                        entityDisplays.Remove(displayCoinToRemove);
                    }
                    break;
                case NetworkListEvent<LeaderboardEntityStateCoin>.EventType.Value:
                    LeaderboardEntityDisplayCoin displayCoinToUpdate =
                        entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                    if (displayCoinToUpdate != null)
                    {
                        displayCoinToUpdate.UpdateCoins(changeEvent.Value.Coins);
                    }
                    break;
            }

            entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));

            for (int i = 0; i < entityDisplays.Count; i++)
            {
                entityDisplays[i].transform.SetSiblingIndex(i);
                entityDisplays[i].UpdateText();
                entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
            }

            LeaderboardEntityDisplayCoin myDisplayCoin =
                entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

            if (myDisplayCoin != null)
            {
                if (myDisplayCoin.transform.GetSiblingIndex() >= entitiesToDisplay)
                {
                    leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                    myDisplayCoin.gameObject.SetActive(true);
                }
            }

            if (!teamLeaderboardBackground.activeSelf) { return; }

            LeaderboardEntityDisplayCoin teamDisplayCoin =
                teamEntityDisplays.FirstOrDefault(x => x.TeamIndex == changeEvent.Value.TeamIndex);

            if (teamDisplayCoin != null)
            {
                if (changeEvent.Type == NetworkListEvent<LeaderboardEntityStateCoin>.EventType.Remove)
                {
                    teamDisplayCoin.UpdateCoins(teamDisplayCoin.Coins - changeEvent.Value.Coins);
                }
                else
                {
                    teamDisplayCoin.UpdateCoins(
                        teamDisplayCoin.Coins + (changeEvent.Value.Coins - changeEvent.PreviousValue.Coins));
                }

                teamEntityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));

                for (int i = 0; i < teamEntityDisplays.Count; i++)
                {
                    teamEntityDisplays[i].transform.SetSiblingIndex(i);
                    teamEntityDisplays[i].UpdateText();
                }
            }
        }

        private void HandlePlayerSpawned(TankPlayer player)
        {
            leaderboardEntities.Add(new LeaderboardEntityStateCoin
            {
                ClientId = player.OwnerClientId,
                PlayerName = player.PlayerName.Value,
                TeamIndex = player.TeamIndex.Value,
                Coins = 0
            });

            player.Wallet.totalCoins.OnValueChanged += (oldCoins, newCoins) =>
                HandleCoinsChanged(player.OwnerClientId, newCoins);
        }

        private void HandlePlayerDespawned(TankPlayer player)
        {
            foreach (LeaderboardEntityStateCoin entity in leaderboardEntities)
            {
                if (entity.ClientId != player.OwnerClientId) { continue; }

                leaderboardEntities.Remove(entity);
                break;
            }

            player.Wallet.totalCoins.OnValueChanged -= (oldCoins, newCoins) =>
                HandleCoinsChanged(player.OwnerClientId, newCoins);
        }

        private void HandleCoinsChanged(ulong clientId, int newCoins)
        {
            for (int i = 0; i < leaderboardEntities.Count; i++)
            {
                if (leaderboardEntities[i].ClientId != clientId) { continue; }

                leaderboardEntities[i] = new LeaderboardEntityStateCoin
                {
                    ClientId = leaderboardEntities[i].ClientId,
                    PlayerName = leaderboardEntities[i].PlayerName,
                    TeamIndex = leaderboardEntities[i].TeamIndex,
                    Coins = newCoins
                };

                return;
            }
        }
    }
}
