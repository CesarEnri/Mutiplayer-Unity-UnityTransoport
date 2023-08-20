using System;
using UnityEngine;

namespace Core.Player
{
    public class PlayerColourDisplay: MonoBehaviour
    {
        [SerializeField] private TeamColourLookup teamColourLookup;

        [SerializeField] private TankPlayer player;
        [SerializeField] private SpriteRenderer[] playerSprites;
        
        private void Start()
        {
            HandleTeamChanged(-1, player.TeamIndex.Value);

            player.TeamIndex.OnValueChanged += HandleTeamChanged;
        }

        private void HandleTeamChanged(int olderTeamIndex, int newTeamIndex)
        {
            var teamColour = teamColourLookup.GetTeamColour(newTeamIndex);

            foreach (var playerSprite in playerSprites)
            {
                playerSprite.color = teamColour;
            }
        }

        private void OnDestroy()
        {
            player.TeamIndex.OnValueChanged -= HandleTeamChanged;
        }
    }
}