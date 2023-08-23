using TMPro;
using Unity.Collections;
using UnityEngine;

namespace UI.Leaderboard
{
    public class LeaderboardEntityDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;

        private FixedString32Bytes _displayName;

        public int TeamIndex { get; private set; }
        public ulong ClientId { get; private set; }
        public int Coins { get; private set; }
        
        public int Kill { get; private set; }
        public int AssistantKill { get; private set; }

        public void Initialise(ulong clientId, FixedString32Bytes displayName, int coins, int kill)
        {
            ClientId = clientId;
            _displayName = displayName;

            UpdateCoins(coins);
            UpdateKill(kill);
        }

        public void Initialise(int teamIndex, FixedString32Bytes displayName, int coins, int kill)
        {
            TeamIndex = teamIndex;
            _displayName = displayName;

            UpdateCoins(coins);
            UpdateKill(kill);
        }

        public void SetColour(Color colour)
        {
            displayText.color = colour;
        }

        public void UpdateCoins(int coins)
        {
            Coins = coins;

            UpdateText();
        }

        private void UpdateKill(int kill)
        {
            Kill = Coins;

            UpdateText();
        }

        public void UpdateText()
        {
            displayText.text = $"{transform.GetSiblingIndex() + 1}. {_displayName} ({Coins}) -- {Kill}";
        }
    }  
}