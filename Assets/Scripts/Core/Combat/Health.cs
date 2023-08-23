using System;
using Core.Player;
using Unity.Netcode;
using UnityEngine;

namespace Core.Combat
{
    public class Health : NetworkBehaviour
    {
        [field: SerializeField] public int MaxHealth { get; private set; } = 100;

        public NetworkVariable<int> currentHealth = new();


        private bool isDead;

        public Action<Health> OnDie;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            currentHealth.Value = MaxHealth;
        }



        public void TakeDamage(int damageValue)
        {
            ModifyHealth(-damageValue);
        }

        public void RestoreHealth(int healValue)
        {
            ModifyHealth(+healValue);
        }

        private void ModifyHealth(int value)
        {
            if(isDead)
                return;

            var newHealth = currentHealth.Value + value;
            currentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);

            if (currentHealth.Value <= 0)
            {
                Debug.Log("You are dead!");
                OnDie?.Invoke(this);
                isDead = true;
                
            }
        }
    }
}
