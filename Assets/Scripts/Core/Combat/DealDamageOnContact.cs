 using Unity.Netcode;
using UnityEngine;

namespace Core.Combat
{
    public class DealDamageOnContact : MonoBehaviour
    {
        [SerializeField] private int damage = 5;

        public void SetOwner(ulong ownerClientId)
        {
            this.ownerClientId = ownerClientId;
        }

        private ulong ownerClientId;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.attachedRigidbody == null) return;
            
            if (other.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                if (ownerClientId == networkObject.OwnerClientId)
                {
                    return;
                }
            }

            if (other.attachedRigidbody.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(damage);
            }
        }
    }
}
