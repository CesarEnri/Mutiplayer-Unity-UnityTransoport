using System;
using System.Threading.Tasks;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Autentication
{
    public class AuthenticationManager : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text testResultLogin;
        
        private void Start()
        {
            InitializePlayGamesLogin();
        }


        private void InitializePlayGamesLogin()
        {
            #if UNITY_ANDROID
            var config = new PlayGamesClientConfiguration.Builder()
                // Requests an ID token be generated.  
                // This OAuth token can be used to
                // identify the player to other services such as Firebase.
                .RequestIdToken()
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
#endif
        }

        public void LoginGoogle()
        {
            Social.localUser.Authenticate(OnGoogleLogin);
        }


        async void OnGoogleLogin(bool success)
        {
#if UNITY_ANDROID
            if (success)
            {
                // Call Unity Authentication SDK to sign in or link with Google.
                Debug.Log("Login with Google done. IdToken: " + ((PlayGamesLocalUser)Social.localUser).GetIdToken());
                
                var token = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
                
                await SignInWithGoogleAsync(token);
            }
            else
            {
                testResultLogin.text = "Unsuccessful login";
                //Debug.Log("Unsuccessful login");
            }
#endif
        }
        
        async Task SignInWithGoogleAsync(string idToken)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithGoogleAsync(idToken);
                
                testResultLogin.text = "SignIn is successful.";
               // Debug.Log("SignIn is successful.");
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                testResultLogin.text = ex.ToString();
                //Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                testResultLogin.text = ex.ToString();
                //Debug.LogException(ex);
            }
        }
    }
}
