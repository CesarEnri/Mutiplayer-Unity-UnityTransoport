using Unity.Netcode;
using UnityEngine;

namespace Core.Coins
{
    public class CoinWallet : NetworkBehaviour
    {
        public NetworkVariable<int> totalCoins = new();

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
    }
}
