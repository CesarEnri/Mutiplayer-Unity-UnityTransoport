using TMPro;
using Unity.Collections;
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
            modeGameQueue.text = gameRule.nameGameQueueMode.Value.Value;
            gameRule.maxCoinsCollect.OnValueChanged += ValueMaxCoinUpdate;
            gameRule.nameGameQueueMode.OnValueChanged += SetValueGameQueue;
        }

        public void SetValueGameQueue(FixedString32Bytes previousvalue, FixedString32Bytes newvalue)
        {
            modeGameQueue.text = newvalue.ToString();
        }

        private void ValueMaxCoinUpdate(int previousvalue, int newvalue)
        {
            maxCoinsDisplay.text = newvalue.ToString();
        }

        private void OnDestroy()
        {
            gameRule.maxCoinsCollect.OnValueChanged -= ValueMaxCoinUpdate;
        }
        

    }
}