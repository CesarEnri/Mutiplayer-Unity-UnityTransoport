using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Coins
{
    public class CoinSpawner : NetworkBehaviour
    {
        [SerializeField] private RespawingCoin coinPrefab;

        [SerializeField] private int maxCoins = 50;

        [SerializeField]
        private int coinValue = 10;
        
        [SerializeField] private Vector2 xSpawnRange;
        [SerializeField] private Vector2 ySpawnRange;

        [SerializeField] private LayerMask layerMask;

        private Collider2D[] coinBuffer = new Collider2D[1];
            
        private float _coinRadius;
        
        public override void OnNetworkSpawn()
        {
            if(!IsServer) return;

            coinPrefab.GetComponent<CircleCollider2D>().radius = _coinRadius;

            for (int i = 0; i < maxCoins; i++)
            {
                SpawnCoin();
            }
        }

        private void SpawnCoin()
        {
            var coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), quaternion.identity);
            
            coinInstance.SetValue(coinValue);
            coinInstance.GetComponent<NetworkObject>().Spawn();

            coinInstance.OnCollected += HandleCoinCollected;
        }

        private void HandleCoinCollected(RespawingCoin coin)
        {
            coin.transform.position = GetSpawnPoint();
            coin.Reset();
        }

        private Vector2 GetSpawnPoint()
        {
            float x = 0;
            float y = 0;

            while (true)
            {
                x = Random.Range(xSpawnRange.x, xSpawnRange.y);
                y = Random.Range(ySpawnRange.x, ySpawnRange.y);
                var spawnPoint = new Vector2(x, y);
                int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, _coinRadius, coinBuffer, layerMask);
                if (numColliders == 0)
                {
                    return spawnPoint;
                }

            }
        }

    }
}
