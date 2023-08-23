using System;
using Unity.Netcode;

namespace Networking.Rules.Component
{
    public class GameRuleState: INetworkSerializable, IEquatable<GameRuleState>
    {
        public int MaxCoins;
        public float TimeGame;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref TimeGame);
            serializer.SerializeValue(ref MaxCoins);
        }

        public bool Equals(GameRuleState other)
        {
            return TimeGame.Equals(other!.TimeGame) && MaxCoins.Equals(other.MaxCoins);
        }
    }
}