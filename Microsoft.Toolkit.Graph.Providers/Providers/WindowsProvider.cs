using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Windows.Security.Authentication.Web;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.ApplicationSettings;

using WebAccount = Windows.Security.Credentials.WebAccount;

namespace Microsoft.Toolkit.Uwp.Graph.Providers
{
    /// <summary>
    /// A provider for leveraging Windows system authentication.
    /// </summary>
    public class WindowsProvider : BaseProvider
    {
        private struct AuthenticatedUser
        {
            public string Token { get; set; }

            public WebAccount Account { get; set; }

            public AuthenticatedUser(string token, WebAccount account)
            {
                Token = token;
                Account = account;
            }
        }

        private const string DefaultTenant = "common";
        private const string WebAccountProviderId = "https://login.microsoft.com";
        private static readonly string[] DefaultScopes = new string[] { "user.read" };
        private static readonly string GraphResourceProperty = "https://graph.microsoft.com";

        public static readonly string RedirectUri = string.Format("ms-appx-web://Microsoft.AAD.BrokerPlugIn/{0}", WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper());

        private AccountsSettingsPane _currentPane;
        private AuthenticatedUser? _currentUser;
        private string[] _scopes;
        private string _clientId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="scopes"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        public static async Task<WindowsProvider> CreateAsync(string clientId, string[] scopes = null)
        {
            var provider = new WindowsProvider(clientId, scopes);
            await provider.TrySilentSignInAsync();
            return provider;
        }

        private WindowsProvider(string clientId, string[] scopes = null)
        {
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _currentPane = null;
            _currentUser = null;
            _scopes = scopes ?? DefaultScopes;

            Graph = new GraphServiceClient(new DelegateAuthenticationProvider(AuthenticateRequestAsync));

            State = ProviderState.SignedOut;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<bool> TrySilentSignInAsync()
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public override Task LoginAsync()
        {
            if (State == ProviderState.SignedIn)
            {
                return Task.CompletedTask;
            }

            if (_currentPane != null)
            {
                _currentPane.AccountCommandsRequested -= BuildPaneAsync;
            }

            _currentPane = AccountsSettingsPane.GetForCurrentView();
            _currentPane.AccountCommandsRequested += BuildPaneAsync;

            AccountsSettingsPane.Show();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task LogoutAsync()
        {
            if (State == ProviderState.SignedOut)
            {
                return Task.CompletedTask;
            }

            if (_currentPane != null)
            {
                _currentPane.AccountCommandsRequested -= BuildPaneAsync;
                _currentPane = null;
            }

            _currentUser = null;
            State = ProviderState.SignedOut;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            // Append the token to the authorization header of any outgoing Graph requests.
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentUser?.Token);
            return Task.CompletedTask;
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/uwp/security/web-account-manager#build-the-account-settings-pane.
        /// </summary>
        private async void BuildPaneAsync(AccountsSettingsPane sender, AccountsSettingsPaneCommandsRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                // Providing nothing shows all accounts, providing authority shows only aad
                var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(WebAccountProviderId);

                if (msaProvider == null)
                {
                    State = ProviderState.SignedOut;
                    return;
                }

                var command = new WebAccountProviderCommand(msaProvider, GetTokenAsync);
                args.WebAccountProviderCommands.Add(command);
            }
            catch
            {
                State = ProviderState.SignedOut;
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void GetTokenAsync(WebAccountProviderCommand command)
        {
            // Build the token request
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, string.Join(',', _scopes), _clientId);
            request.Properties.Add("resource", GraphResourceProperty);

            // Get the results
            WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);

            // Handle user cancellation
            if (result.ResponseStatus == WebTokenRequestStatus.UserCancel)
            {
                State = ProviderState.SignedOut;
                return;
            }

            // Handle any errors
            if (result.ResponseStatus != WebTokenRequestStatus.Success)
            {
                Debug.WriteLine(result.ResponseError.ErrorMessage);
                State = ProviderState.SignedOut;
                return;
            }

            // Extract values from the results
            var account = result.ResponseData[0].WebAccount;
            var token = result.ResponseData[0].Token;

            // Set the current user object
            _currentUser = new AuthenticatedUser(token, account);

            // Update the state to be signed in.
            State = ProviderState.SignedIn;
        }
    }
}
