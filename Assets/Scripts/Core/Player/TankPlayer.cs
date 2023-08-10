using System;
using Cinemachine;
using Core.Combat;
using Networking.Host;
using Networking.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [field: SerializeField] public Health Health { get; private set; }

        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;

        public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

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
