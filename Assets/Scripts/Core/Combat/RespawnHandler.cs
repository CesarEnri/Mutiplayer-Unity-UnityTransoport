﻿using System;
using System.Collections;
using Core.Player;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Core.Combat
{
    public class RespawnHandler : NetworkBehaviour
    {
        [SerializeField] private TankPlayer playerPrefab;
        [SerializeField] private float keptCoinPercentage;

        public TankPlayer killer;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) { return; }

            TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (TankPlayer player in players)
            {
                HandlePlayerSpawned(player);
            }

            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) { return; }

            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }

        private void HandlePlayerSpawned(TankPlayer player)
        {
            player.Health.OnDie += (health) => HandlePlayerDie(player);
        }

        private void HandlePlayerDespawned(TankPlayer player)
        {
            player.Health.OnDie -= (health) => HandlePlayerDie(player);
        }

        private void HandlePlayerDie(TankPlayer player)
        {
            var keptCoins = (int)(player.Wallet.totalCoins.Value * (keptCoinPercentage / 100));
            var keptKill = player.KillTrunk.totalKills.Value;
            
            Destroy(player.gameObject);

            StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins, keptKill));
        }

        private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins, int keptKill)
        {
            yield return null;

            var playerInstance = Instantiate(
                playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

            playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);
            
            playerInstance.Wallet.totalCoins.Value += keptCoins;

            playerInstance.KillTrunk.totalKills.Value = keptKill;
        }
    }

}