using System;
using Networking.Client;
using Networking.Host;
using TMPro;
using UnityEngine;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Text queueStatusText;
        [SerializeField] private TMP_Text queueTimerText;
        [SerializeField] private TMP_Text findMatchButtonText;


        [SerializeField] private TMP_InputField joinCodeField;

        private bool _isMatchmaking;
        private bool _isCancelling;
        
        private void Start()
        {
            if (ClientSingleton.Instance == null) return;

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            queueStatusText.text = string.Empty;
            queueTimerText.text = string.Empty;
        }

        public async void FindMatchPressed()
        {
            if(_isCancelling) return;
            
            if (_isMatchmaking)
            {
                queueStatusText.text = "Cancelling...";
                _isCancelling = false;
                //cancel
                
                _isCancelling = false;
                _isMatchmaking = false;
                findMatchButtonText.text = "Find Match";
                queueStatusText.text = string.Empty;
                return;

            }

            
            //Start queue
            findMatchButtonText.text = "Cancel";
            queueStatusText.text = "Seaching...";
            _isMatchmaking = true;
            
        }



        public async void StartHost()
        {
            await HostSingleton.Instance.HostGameManager.StartHostAsync();
        }

        public async void StartClient()
        {
            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
        }
    }
}