using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Leaderboard
{
    public class Leaderboard : NetworkBehaviour
    {
        [SerializeField] private Transform leaderboardEntityHolder;
        [SerializeField] private LeaderboardEntityDisplay leaderboardEntityDisplayPrefab;

        private NetworkList<LeaderboardEntityState> _leaderboardEntities;

        private void Awake()
        {
            _leaderboardEntities = new NetworkList<LeaderboardEntityState>();
        }
    }
}
