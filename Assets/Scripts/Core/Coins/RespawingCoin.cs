namespace Core.Coins
{
    public class RespawingCoin : Coin
    {
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

            return CoinValue;
        }
    }
}
