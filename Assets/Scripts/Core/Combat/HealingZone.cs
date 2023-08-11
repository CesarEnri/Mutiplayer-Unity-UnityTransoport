using System;
using System.Collections.Generic;
using Core.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Combat
{
    public class HealingZone: NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Image healPowerBar;

        [Header("Settings")] [SerializeField] private int maxHealPower = 30;
        [SerializeField] private float healCooldown = 60f;
        [SerializeField] private float healTickRate = 1f;
        [SerializeField] private int coinsPerTick = 10;
        [SerializeField] private int healthPerTick = 10;

        private float _remainingCooldown;
        private float _tickTimer;
        

        private List<TankPlayer> _playerInZone = new();

        private NetworkVariable<int> HealtPower = new();

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                HealtPower.OnValueChanged += HandleHealPowerChanged;
                HandleHealPowerChanged(0, HealtPower.Value);
            }

            if (IsServer)
            {
                HealtPower.Value = maxHealPower;
            }
            else
            {
                return;
            }

            _tickTimer += Time.deltaTime;
            if (_tickTimer >= 1/  healTickRate)
            {
                foreach (var player in _playerInZone)
                {
                    if (HealtPower.Value == 0)
                    {
                        break;
                    }
                    
                    if(player.Health.currentHealth.Value == player.Health.MaxHealth) continue;
                    
                    if(player.Wallet.totalCoins.Value < coinsPerTick) continue;
                    
                    player.Wallet.SpendCoins(coinsPerTick);
                    player.Health.RestoreHealth(healthPerTick);

                    HealtPower.Value -= 1;

                    if (HealtPower.Value <= 0)
                    {
                        _remainingCooldown = healCooldown;
                    }
                }

                _tickTimer = _tickTimer % (1/healTickRate);
            }
            

        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                HealtPower.OnValueChanged -= HandleHealPowerChanged;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!IsServer) return;
            
            if(!other.attachedRigidbody.TryGetComponent(out TankPlayer player)) return;
            
            _playerInZone.Add(player);
            
          
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if(!IsServer) return;
            
            
            if(!other.attachedRigidbody.TryGetComponent(out TankPlayer player)) return;
            
            _playerInZone.Remove(player);
         
        }

        private void Update()
        {
            if(!IsServer) return;

            if (_remainingCooldown > 0f)
            {
                _remainingCooldown -= Time.deltaTime;

                if (_remainingCooldown <= 0f)
                {
                    HealtPower.Value = maxHealPower;
                }
            }

        }

        private void HandleHealPowerChanged(int oldHealPower, int newHealPower)
        {
            healPowerBar.fillAmount = (float)newHealPower / maxHealPower;
        }
    }
}