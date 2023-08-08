using Core.Coins;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> totalCoins;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.TryGetComponent(out Coin coin))
            return;

        int coinValue = coin.Collect();
        coin.Show(false);

        if (!IsServer) return;

        totalCoins.Value += coinValue;
    }
}
