using System;
using Cinemachine;
using Core.Coins;
using Core.Combat;
using Core.Kill;
using Networking.Host;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        [SerializeField] private SpriteRenderer minimapIconRender;
        [SerializeField] private Texture2D crossHair;
        
        [field: SerializeField] public Health Health { get; private set; }
        [field: SerializeField] public CoinWallet Wallet { get; private set; }
        [field: SerializeField] public KillTrunk KillTrunk { get; private set; }

        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;

        [SerializeField] private Color ownerColour;

        public NetworkVariable<FixedString32Bytes> PlayerName = new();
        public NetworkVariable<int> TeamIndex = new();

        [SerializeField] private PlayerMovement playerMovement;
        
        public static event Action<TankPlayer> OnPlayerSpawned;
        public static event Action<TankPlayer> OnPlayerDespawned;

        public UserData userDataInformation;
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                UserData userData = null;
                if (IsHost)
                {
                    userData= HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientID(OwnerClientId);
                }
                else
                {
                    userData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientID(OwnerClientId);
                }
                
                PlayerName.Value = userData.userName;
                TeamIndex.Value = userData.teamIndex;
                
                OnPlayerSpawned?.Invoke(this);

                userDataInformation = userData;
            }

            if (IsOwner)
            {
                virtualCamera.Priority = ownerPriority;
                minimapIconRender.color = ownerColour;
                
                Cursor.SetCursor(crossHair, new Vector2(crossHair.width / 2, crossHair.height / 2), CursorMode.Auto);
            }
            
        }


        // public void HandlePlayerMovement(bool canMove)
        // {
        //     playerMovement.HandleMovementPlayer(canMove);
        // }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                OnPlayerDespawned?.Invoke(this);
            }
        }
    }
}
