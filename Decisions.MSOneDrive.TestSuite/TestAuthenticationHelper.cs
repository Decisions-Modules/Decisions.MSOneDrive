using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Decisions.MSOneDrive;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Decisons.MSOneDrive.TestSuite
{
    public class TestAuthenticationHelper
    {
        // The Client ID is used by the application to uniquely identify itself to the v2.0 authentication endpoint.
        static string clientId = "";
        public static string[] Scopes = { "Files.ReadWrite.All" };

        public static IPublicClientApplication IdentityClientApp = null; 

        public static string TokenForUser = null;
        public static DateTimeOffset Expiration;

        private static GraphServiceClient graphClient = null;
         

        // Get an access token for the given context and resourceId. An attempt is first made to 
        // acquire the token silently. If that fails, then we try to acquire the token by prompting the user.
        public static GraphServiceClient GetAuthenticatedClient(string clientId = TestData.ClientOrAppId)
        {
            if (graphClient == null)
            {
                TestAuthenticationHelper.clientId = clientId;

                IdentityClientApp = PublicClientApplicationBuilder.Create(clientId).Build();//https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-interactively

                var accounts = IdentityClientApp.GetAccountsAsync().Result;

                AuthenticationResult result;
                try
                {
                    result = IdentityClientApp.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                                .ExecuteAsync().Result;
                }
                catch (MsalUiRequiredException)
                {
                    result = IdentityClientApp.AcquireTokenInteractive(Scopes)
                                .ExecuteAsync().Result;
                }

                graphClient = AuthenticationHelper.GetAuthenticatedClient(result.AccessToken);
            }

            return graphClient;
        }


  /*      /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> GetTokenForUserAsync()
        {
            AuthenticationResult authResult;
            try
            {
                authResult = await IdentityClientApp.AcquireTokenSilentAsync(Scopes);
                TokenForUser = authResult.Token;
            }

            catch (Exception)
            {
                if (TokenForUser == null || Expiration <= DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    authResult = await IdentityClientApp.AcquireTokenAsync(Scopes);

                    TokenForUser = authResult.Token;
                    Expiration = authResult.ExpiresOn;
                }
            }

            return TokenForUser;
        }

        /// <summary>
        /// Signs the user out of the service.
        /// </summary>
        public static void SignOut()
        {
            foreach (var user in IdentityClientApp.Users)
            {
                user.SignOut();
            }
            graphClient = null;
            TokenForUser = null;

        }*/

    }
}
