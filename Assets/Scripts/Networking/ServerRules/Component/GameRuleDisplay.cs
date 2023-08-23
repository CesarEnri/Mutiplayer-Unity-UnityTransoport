using System;
using TMPro;
using UnityEngine;

namespace Networking.ServerRules.Component
{
    public class GameRuleDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text modeGameQueue;
        [SerializeField] private TMP_Text maxCoinsDisplay;

        [SerializeField] private GameRule gameRule;
        
        private void Awake()
        {
            modeGameQueue.text = gameRule.gameQueueMode.Value.ToString();
            gameRule.timeRuleInt.OnValueChanged += ValueMaxCoinUpdate;


            gameRule.actionOnClient += HandleClientConnect;
        }

        private void HandleClientConnect(GameRule gameRule)
        {
            modeGameQueue.text = gameRule.gameQueueMode.Value.ToString();
        }
        
        private void ValueMaxCoinUpdate(int previousvalue, int newvalue)
        {
            //maxCoinsDisplay.text = newvalue.ToString();
            var  ts = TimeSpan.FromSeconds(newvalue);
            maxCoinsDisplay.text = $"{ts.Minutes:0}:{ts.Seconds:00}";
        }

        private void OnDestroy()
        {
            gameRule.timeRuleInt.OnValueChanged -= ValueMaxCoinUpdate;
        }
        

    }
}