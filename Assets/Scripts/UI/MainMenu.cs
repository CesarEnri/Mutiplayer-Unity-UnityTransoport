using System;
using Networking.Client;
using Networking.Host;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Cursor = UnityEngine.Cursor;
using Toggle = UnityEngine.UI.Toggle;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Text queueStatusText;
        [SerializeField] private TMP_Text queueTimerText;
        [SerializeField] private TMP_Text findMatchButtonText;
        [SerializeField] private Toggle teamToggle;
        [SerializeField] private Toggle privateToggle;
        

        [SerializeField] private TMP_InputField joinCodeField;

        private bool _isMatchmaking;
        private bool _isCancelling;
        private bool _isBusy;
        private float _timeInQueue;
        
        private void Start()
        {
            if (ClientSingleton.Instance == null) return;

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            queueStatusText.text = string.Empty;
            queueTimerText.text = string.Empty;
        }

        private void Update()
        {
            if (_isMatchmaking)
            {
                _timeInQueue += Time.deltaTime;
                var  ts = TimeSpan.FromSeconds(_timeInQueue);
                queueTimerText.text = string.Format("{0:0}:{1:00}", ts.Minutes, ts.Seconds);
            }
        }

        public async void FindMatchPressed()
        {
            if(_isCancelling) return;
            
            if (_isMatchmaking)
            {
                queueStatusText.text = "Cancelling...";
                _isCancelling = false;
                await ClientSingleton.Instance.GameManager.CancelMatchmaking();
                _isCancelling = false;
                _isMatchmaking = false;
                _isBusy = false;
                findMatchButtonText.text = "Find Match";
                queueStatusText.text = string.Empty;
                queueTimerText.text = string.Empty;
                return;

            }
            
            if(_isBusy) return;
            
            ClientSingleton.Instance.GameManager.MatchmakerAsync(teamToggle.isOn,  OnMatchMade);
            findMatchButtonText.text = "Cancel";
            queueStatusText.text = "Searching...";
            _timeInQueue = 0f;
            _isMatchmaking = true;
            
        }

        private void OnMatchMade(MatchmakerPollingResult result)
        {
            switch (result)
            {
                case MatchmakerPollingResult.Success:
                    queueStatusText.text = "Connecting...";
                    break;
                case MatchmakerPollingResult.TicketCreationError:
                    queueStatusText.text = "TicketCreationError";
                    break;
                case MatchmakerPollingResult.TicketCancellationError:
                    queueStatusText.text = "TicketCancellationError";
                    break;
                case MatchmakerPollingResult.TicketRetrievalError:
                    queueStatusText.text = "TicketRetrievalError";
                    break;
                case MatchmakerPollingResult.MatchAssignmentError:
                    queueStatusText.text = "MatchAssignmentError";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }


        public async void StartHost()
        {
            if(_isBusy) return;

            _isBusy = true;
            
            await HostSingleton.Instance.HostGameManager.StartHostAsync(privateToggle.isOn);
            
            _isBusy = false;
        }

        public async void StartClient()
        {
            if(_isBusy) return;

            _isBusy = true;
            
            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
            
            _isBusy = false;
        }

        public async void JoinAsync(Lobby lobby)
        {
            if (_isBusy) { return; }

            _isBusy = true;

            try
            {
                var joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
                var joinCode = joiningLobby.Data["JoinCode"].Value;

                await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            _isBusy = false;
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}