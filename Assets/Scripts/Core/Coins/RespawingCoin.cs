using System;
using UnityEngine;

namespace Core.Coins
{
    public class RespawingCoin : Coin
    {
        public event Action<RespawingCoin> OnCollected;

        private Vector3 _previousPosition;
        
        private void Update()
        {
            if (_previousPosition != transform.position)
            {
                Show(true);
            }

            _previousPosition = transform.position;
        }

        public override int Collect()
        {
            if (!IsServer)
            {
                Show(false);
                return 0;
            }

            if (AlreadyCollected)
                return 0;

            AlreadyCollected = true;
            
            OnCollected?.Invoke(this);

            return CoinValue;
        }

        public void Reset()
        {
            AlreadyCollected = false;
        }
    }
}
