using System;
using Cinemachine;
using Core.Coins;
using Core.Combat;
using Networking.Host;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        [SerializeField] private SpriteRenderer minimapIconRender;
        
        [field: SerializeField] public Health Health { get; private set; }
        [field: SerializeField] public CoinWallet Wallet { get; private set; }

        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;

        [SerializeField] private Color ownerColour;

        public NetworkVariable<FixedString32Bytes> PlayerName = new();

        public static event Action<TankPlayer> OnPlayerSpawned;
        public static event Action<TankPlayer> OnPlayerDespawned;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                var userData =
                    HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientID(OwnerClientId);

                PlayerName.Value = userData.userName;

                OnPlayerSpawned?.Invoke(this);
            }

            if (IsOwner)
            {
                virtualCamera.Priority = ownerPriority;
                minimapIconRender.color = ownerColour;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                OnPlayerDespawned?.Invoke(this);
            }
        }
    }
}
