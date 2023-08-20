using Core.Coins;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace UI.Leaderboard
{
    public class LeaderboardEntityDisplay: MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;

        private FixedString32Bytes displayName;
        public int TeamIndex { get; private set; }

        public ulong ClientId { get; private set; }
        public int Coins { get; private set;}

        public void Initialise(ulong clientId, FixedString32Bytes playerName, int coins)
        {
            ClientId = clientId;
            displayName = playerName;
            
            UpdateCoins(coins);
        }

        public void SetColour(Color colour)
        {
            displayText.color = colour;
        }

        public void Initialise(int teamIndex, FixedString32Bytes playerName, int coins)
        {
            TeamIndex = teamIndex;
            displayName = playerName;
            
            UpdateCoins(coins);
        }

        public void UpdateCoins(int coins)
        {
            Coins = coins;
            UpdateText();
        }

        public void UpdateText()
        {
            displayText.text = $"{transform.GetSiblingIndex() + 1}. {displayName} ({Coins})"; 
        }
    }
}