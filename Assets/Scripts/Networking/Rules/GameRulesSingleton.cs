using System.Threading.Tasks;
using Core.Rules;
using UnityEngine;

namespace Networking.Rules
{
    public class GameRulesSingleton : MonoBehaviour
    {
        private static GameRulesSingleton instance;
        public GameRulesManager GameRules { get; private set; }

        public static GameRulesSingleton Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = FindObjectOfType<GameRulesSingleton>();

                if (instance == null)
                {
                    return null;
                }

                return instance;
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async Task CreateRules()
        {
            //set rules
        }

        private void OnDestroy()
        {
            //GameRules?.Dispose();
        }
    }
}