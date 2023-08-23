using System;
using Unity.Collections;
using Unity.Netcode;

namespace UI.Leaderboard
{
    public struct LeaderboardEntityState : INetworkSerializable, IEquatable<LeaderboardEntityState>
    {
        public ulong ClientId;
        public int TeamIndex;
        public FixedString32Bytes PlayerName;
        public int Coins;
        public int Kill;
        public int AssistantKill;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref TeamIndex);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref Coins);
            serializer.SerializeValue(ref Kill);
            serializer.SerializeValue(ref AssistantKill);
        }

        public bool Equals(LeaderboardEntityState other)
        {
            return ClientId == other.ClientId &&
                   TeamIndex == other.TeamIndex &&
                   PlayerName.Equals(other.PlayerName) &&
                   Coins == other.Coins &&
                   Kill == other.Kill &&
                   AssistantKill == other.AssistantKill;
        }
    }
}