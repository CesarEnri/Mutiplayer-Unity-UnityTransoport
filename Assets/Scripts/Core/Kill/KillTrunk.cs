using Core.Coins;
using Core.Combat;
using Unity.Netcode;
using UnityEngine;

namespace Core.Kill
{
    public class KillTrunk: NetworkBehaviour
    {
        [SerializeField] private Health health;
        
        public NetworkVariable<int> totalKills;
        
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!other.TryGetComponent(out Projectile projectile))
                return;

            Debug.Log("WEO");
            Debug.Log(health.currentHealth.Value);
            if (health.currentHealth.Value <= 100)
            {
                Debug.Log("sin");

                if (IsServer || IsHost)
                {
                    Debug.Log("vida");

                    projectile.tankPlayer.KillTrunk.totalKills.Value += 1;    
                }

            }
        }
        
    }
}