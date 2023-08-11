using Core.Combat;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Coins
{
    public class CoinWallet : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Health health;
        [SerializeField] private BountyCoin coinPrefab;


        [Header("Settings")] 
        [SerializeField] private float coinSpread = 3f;
        [SerializeField] private float bountyPercentage = 50f;
        [SerializeField]private int bountyCoinCount = 10;
        [SerializeField] private int minBountyCoinValue = 5;
        [SerializeField] private LayerMask layerMask;

        private Collider2D[] coinBuffer = new Collider2D[1];
            
        private float _coinRadius;
        
        public NetworkVariable<int> totalCoins = new();

        public override void OnNetworkSpawn()
        {
            if(!IsServer) return;
            
            coinPrefab.GetComponent<CircleCollider2D>().radius = _coinRadius;

            health.OnDie += HandleDie;
        }

        public override void OnNetworkDespawn()
        {
            if(!IsServer) return;

            health.OnDie += HandleDie;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!other.TryGetComponent(out Coin coin))
                return;

            int coinValue = coin.Collect();
            coin.Show(false);

            if (!IsServer) return;

            totalCoins.Value += coinValue;
        }

        public void SpendCoins(int value)
        {
            totalCoins.Value -= value;
        }
        
        
        private void HandleDie(Health health)
        {
            var bountyValue = (int)(totalCoins.Value * (bountyPercentage / 100));

            var bountyCoinValue = bountyValue / bountyCoinCount;
            
            if(bountyCoinValue < minBountyCoinValue) return;

            for (int i = 0; i < bountyCoinCount; i++)
            {
                var coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), quaternion.identity);
                coinInstance.SetValue(bountyCoinValue);
                coinInstance.NetworkObject.Spawn();
            }
        }
        
        
        private Vector2 GetSpawnPoint()
        {
            while (true)
            {
                var spawnPoint = (Vector2)transform.position + Random.insideUnitCircle * coinSpread;
                int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, _coinRadius, coinBuffer, layerMask);
                if (numColliders == 0)
                {
                    return spawnPoint;
                }

            }
        }



    }
}
