using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Client
{
    public class ClientSingleton: MonoBehaviour
    {
        private static ClientSingleton instance;
        private ClientGameManager _clientGameManager;

        private static ClientSingleton Instace
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

        public async Task CreateClient()
        {
            _clientGameManager = new ClientGameManager();

            await _clientGameManager.InitAsync();
        }
    }
}