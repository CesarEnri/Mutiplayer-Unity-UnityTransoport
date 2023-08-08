using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Networking.Client
{
    public static class AuthenticationWrapper
    {
        public static AuthState AuthState { get; private set; } = AuthState.NoAuthenticated;

        public static async Task<AuthState> DoAuth(int maxRetries = 5)
        {
            if (AuthState == AuthState.Authenticated)
            {
                return AuthState;
            }

            if (AuthState == AuthState.Authenticating)
            {
                Debug.LogWarning("Already authenticating!");
                await Authenticating();
                return AuthState;
            }

            await SignInAnonymouslyAsync(maxRetries);

            return AuthState;
        }

        private static async Task<AuthState> Authenticating()
        {
            while (AuthState  == AuthState.Authenticating || AuthState == AuthState.NoAuthenticated)
            {
                await Task.Delay(200);
            }

            return AuthState;
        }


        private static async Task SignInAnonymouslyAsync(int maxRetries)
        { 
            AuthState = AuthState.Authenticating;
            
            int retries = 0;
            while (AuthState ==  AuthState.Authenticating && retries < maxRetries)
            {
                try
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                    if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                    {
                        AuthState = AuthState.Authenticated;
                        break;
                    }
                }
                catch (AuthenticationException exception)
                {
                    Debug.Log(exception);
                    AuthState = AuthState.Error;
                    throw;
                }
                catch (RequestFailedException exception)
                {
                    Debug.Log(exception);
                    AuthState = AuthState.Error;
                }

                retries++;
                await Task.Delay(1000);
            }

            if (AuthState != AuthState.Authenticated)
            {
                Debug.LogWarning($"Player was not signed in successfully after {retries} tries");
                AuthState = AuthState.TimeOut;
            }
        }
    }

    public enum AuthState
    {
        NoAuthenticated,
        Authenticating,
        Authenticated,
        Error,
        TimeOut,
    }
}
