using System;
using Unity.Collections;
using Unity.Netcode;

namespace UI.Leaderboard
{
    public struct LeaderboardEntityStateCoin : INetworkSerializable, IEquatable<LeaderboardEntityStateCoin>
    {
        public ulong ClientId;
        public int TeamIndex;
        public FixedString32Bytes PlayerName;
        public int Coins;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref TeamIndex);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref Coins);
        }

        public bool Equals(LeaderboardEntityStateCoin other)
        {
            return ClientId == other.ClientId &&
                   TeamIndex == other.TeamIndex &&
                   PlayerName.Equals(other.PlayerName) &&
                   Coins == other.Coins;
        }
    }
}