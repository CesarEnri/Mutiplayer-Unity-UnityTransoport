using Unity.Netcode;
using UnityEngine;

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

        private void SpawnCoin()
        {
            Instantiate(coinPrefab)
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
                
            }
        }

    }
}
