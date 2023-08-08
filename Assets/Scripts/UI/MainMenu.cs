using Networking.Client;
using Networking.Host;
using TMPro;
using UnityEngine;

namespace UI
{
    public class MainMenu: MonoBehaviour
    {
        [SerializeField] private TMP_InputField joinCOdeField;

        public async void StartHostAsync()
        {
            await HostSingleton.Instance.HostGameManager.StartHostAsync();
        }

        public async void StartClientAsync()
        {
            await ClientSingleton.Instance.ClientGameManager.StartClientAsync(joinCOdeField.text);
        }
    }
}