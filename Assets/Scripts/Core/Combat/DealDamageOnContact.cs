 using Core.Player;
 using UnityEngine;

namespace Core.Combat
{
    public class DealDamageOnContact : MonoBehaviour
    {
        [SerializeField] private Projectile projectile;
        
        [SerializeField] private int damage = 5;

     //   private ulong ownerClientId;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.attachedRigidbody == null) { return; }

            if (projectile.TeamIndex != -1)
            {
                if (other.attachedRigidbody.TryGetComponent(out TankPlayer player))
                {
                    if (player.TeamIndex.Value == projectile.TeamIndex)
                    {
                        return;
                    }
                }
            }

            if (other.attachedRigidbody.TryGetComponent(out Health health))
            {
                
                health.TakeDamage(damage);
            }

        }
    }
}
