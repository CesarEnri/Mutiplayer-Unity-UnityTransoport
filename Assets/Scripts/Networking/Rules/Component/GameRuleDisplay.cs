using TMPro;
using UnityEngine;

namespace Networking.Rules.Component
{
    public class GameRuleDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text maxCoinsDisplay;

        [SerializeField] private GameRule gameRule;
        
        private void Start()
        {
            gameRule.MaxCoinsCollect.OnValueChanged += ValueMaxCoinUpdate;
        }

        private void ValueMaxCoinUpdate(int previousvalue, int newvalue)
        {
            maxCoinsDisplay.text = newvalue.ToString();
        }

        private void OnDestroy()
        {
            gameRule.MaxCoinsCollect.OnValueChanged -= ValueMaxCoinUpdate;
        }

        // [SerializeField] private TMP_Text maxCoinsDisplay;
        //
        //
        // public float TimeGame { get; private set; }
        // public float MaxCoins { get; private set; }
        //
        // public void Initialise(float timeGame, int maxCoins)
        // {
        //     TimeGame = timeGame;
        //     MaxCoins = maxCoins;
        // }
        //
        // private void UpdateText()
        // {
        //     timeDisplay.text = TimeGame.ToString(CultureInfo.InvariantCulture);
        //     maxCoinsDisplay.text = MaxCoins.ToString(CultureInfo.InvariantCulture);
        // }


    }
}