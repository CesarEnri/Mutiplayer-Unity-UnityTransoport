using Core.Combat;
using Unity.Netcode;
using UnityEngine;

namespace Core.Kill
{
    public class KillTrunk: NetworkBehaviour
    {
        [SerializeField] private int pointsForKill = 5;
        
        [SerializeField] private Health health;
        
        public NetworkVariable<int> totalKills = new();


        public override void OnNetworkSpawn()
        {
            if (IsServer || IsHost)
            {
                totalKills.Value = 0;
            }
        }

        public override void OnNetworkDespawn()
        {
            
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!other.TryGetComponent(out Projectile projectile))
                return;
            
            if (health.currentHealth.Value <= 0)
            {
                if (IsServer || IsHost)
                {
                    projectile.tankPlayer.KillTrunk.totalKills.Value += pointsForKill;
                }

            }
        }
        
    }
}