using Cinemachine;
using Networking.Host;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        
        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;


        public NetworkVariable<FixedString32Bytes> playerName = new();
        
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                var userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientID(OwnerClientId);

                playerName.Value = userData.userName;
            }



            if (IsOwner)
            {
                virtualCamera.Priority = ownerPriority;
            }
        }
    }
}
