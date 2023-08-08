using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Client
{
    public class ClientSingleton: MonoBehaviour
    {
        private static ClientSingleton instance;
        public ClientGameManager ClientGameManager { get; private set; }

        public static ClientSingleton Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = FindObjectOfType<ClientSingleton>();

                if (instance == null)
                {
                    Debug.LogError("No ClientSingleton in the scene!");
                    return null;
                }

                return instance;
            }
        }
        

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async Task<bool> CreateClient()
        {
            ClientGameManager = new ClientGameManager();

            var result =  await ClientGameManager.InitAsync();
            return result;
        }
    }
}