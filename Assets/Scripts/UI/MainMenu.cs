using System;
using Networking.Client;

using Networking.Host;

using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;
using Toggle = UnityEngine.UI.Toggle;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Text queueStatusText;
        [SerializeField] private TMP_Text queueTimerText;
        
        [SerializeField] private Button findMatchButtonText;
        [SerializeField] private Button findTeamMatchButtonText;
        //[SerializeField] private Toggle teamToggle;
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
                queueTimerText.text = $"{ts.Minutes:0}:{ts.Seconds:00}";
            }
        }

        private GameQueue _selectedGameSoloQueue;

        [SerializeField] private TMP_Dropdown gameQueueMode;
        
        public void ValidateGameQueueSelected()
        {
            _selectedGameSoloQueue = gameQueueMode.options[gameQueueMode.value].text switch
            {
                "DeathMatch" => GameQueue.SoloDeathMatch,
                "TimeMatch" => GameQueue.SoloTimeMatch,
                "PointsMatch" => GameQueue.SoloPointsMatch,
                "Solo" => GameQueue.Solo,
                "Team" => GameQueue.Team,
                _ => _selectedGameSoloQueue
            };
            
            Debug.Log(_selectedGameSoloQueue);
            //ServerSingleton.Instance.gameQueue = _selectedGameSoloQueue;
            ClientSingleton.Instance.gameQueue = _selectedGameSoloQueue;
            HostSingleton.Instance.gameQueue = _selectedGameSoloQueue;
        }

        public async void FindMatchPressed(bool modeParty)
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
                if (!modeParty)
                    findMatchButtonText.GetComponentInChildren<TMP_Text>().text ="SOLO";
                else
                    findTeamMatchButtonText.GetComponentInChildren<TMP_Text>().text = "TEAM";
                findMatchButtonText.enabled = true;
                findTeamMatchButtonText.enabled = true;
                
                queueStatusText.text = string.Empty;
                queueTimerText.text = string.Empty;
                return;
            }
            
            if(_isBusy) return;
            
            ClientSingleton.Instance.GameManager.MatchmakerAsync(_selectedGameSoloQueue,  OnMatchMade);
            
            if (!modeParty)
            {
                findMatchButtonText.GetComponentInChildren<TMP_Text>().text = "Cancel";
                findTeamMatchButtonText.enabled = false;
            }
            else
            {
                findTeamMatchButtonText.GetComponentInChildren<TMP_Text>().text = "Cancel";
                findMatchButtonText.enabled = false;
            }
            
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
                    queueStatusText.text = "MatchAssignmentError"; //Error generado por una mala configuracion en el servicio
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