using System;
using Core.Coins;
using Core.Combat;
using Input;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Player
{
    public class ProjectileLauncher : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;

        [SerializeField] private CoinWallet coinWallet;
        [SerializeField] private Transform projectSpawnPoint;
        [SerializeField] private GameObject serverProjectilePrefab;
        [SerializeField] private GameObject clientProjectilePrefab;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private Collider2D playerCollider;

        [Header("Settings")] [SerializeField] private float projectileSpeed;
        [SerializeField] private float fireRate;
        [SerializeField] private float muzzleFlashDuration;
        [SerializeField] private int costToFire;

        private bool _isPointOverUi; 
        private bool _shouldFire;
        private float timer;

        private float _muzzleFlashTimer;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            inputReader.PrimaryFireEvent += HandlePrimaryFire;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;

            inputReader.PrimaryFireEvent -= HandlePrimaryFire;
        }

        private void HandlePrimaryFire(bool shouldFire)
        {
            if (_shouldFire)
            {
                if(_isPointOverUi) return;
            }

            _shouldFire = shouldFire;
        }

        private void Update()
        {
            
            if (_muzzleFlashTimer > 0f)
            {
                _muzzleFlashTimer -= Time.deltaTime;

                if (_muzzleFlashTimer <= 0f)
                {
                    muzzleFlash.SetActive(false);
                }
            }

            if (!IsOwner) return;

            _isPointOverUi = EventSystem.current.IsPointerOverGameObject();

            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }

            if(!_shouldFire) return;
            
            if(timer > 0) return;
            
            if(coinWallet.totalCoins.Value < costToFire)
                return;

            PrimaryFireServerRpc(projectSpawnPoint.position, projectSpawnPoint.up);
            
            SpawnDummyProjectile(projectSpawnPoint.position, projectSpawnPoint.up);
            
            timer = 1 / fireRate;
        }
        
        [ServerRpc]
        private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
        {
            if(coinWallet.totalCoins.Value < costToFire)
                return;

            coinWallet.SpendCoins(costToFire);
            
            var projectile = Instantiate(serverProjectilePrefab, spawnPos, quaternion.identity);

            projectile.transform.up = direction;
            
            Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());

            if (projectile.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamageOnContact))
            {
                dealDamageOnContact.SetOwner(OwnerClientId);
            }

            if (projectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.velocity = rb.transform.up * projectileSpeed;
            }
            
            SpawnDummyProjectileClientRpc(spawnPos, direction);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        [ClientRpc]
        private void SpawnDummyProjectileClientRpc(Vector3 spawnPoint, Vector3 direction)
        {
            if(IsOwner) return;

            SpawnDummyProjectile(spawnPoint, direction);
        }
        
        private void SpawnDummyProjectile(Vector3 spawnPoint, Vector3 direction)
        {
            muzzleFlash.SetActive(true);
            _muzzleFlashTimer = muzzleFlashDuration;
            
            var projectile = Instantiate(clientProjectilePrefab, spawnPoint, quaternion.identity);

            projectile.transform.up = direction;
            
            Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());

            if (projectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.velocity = rb.transform.up * projectileSpeed;
            }
        }

    }
}
