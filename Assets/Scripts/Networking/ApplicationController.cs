using System.Threading.Tasks;
using Networking.Client;
using Networking.Host;
using Networking.Server;
using UnityEngine;

namespace Networking
{
    public class ApplicationController: MonoBehaviour
    {
        [SerializeField] private ClientSingleton clientPrefab;
        [SerializeField] private HostSingleton hostPrefab;
        [SerializeField] private ServerSingleton serverPrefab;

        public const string ConfigProtocol = "udp";//dtls

        private ApplicationData _applicationData;
        
        private async void Start()
        {
            DontDestroyOnLoad(gameObject);
            await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        }
        
        private async Task LaunchInMode(bool isDedicatedServer)
        {
            if (isDedicatedServer)
            {
                _applicationData = new ApplicationData();
                var serverSingleton = Instantiate(serverPrefab);
                await serverSingleton.CreateServer();

                await serverSingleton.GameManager.StartGameServerAsync();
              
            }
            else
            {
                var hostSingleton = Instantiate(hostPrefab);
                hostSingleton.CreateHost();
                
                var clientSingleton = Instantiate(clientPrefab);
                var autenticated = await clientSingleton.CreateClient();

                if (autenticated)
                {
                    ClientGameManager.GoToMenu();
                }

                //Go to Main Menu

            }
            
        }


    }
}