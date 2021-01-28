using Microsoft.Graph;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseProvider : IProvider
    {
        private ProviderState _state;

        /// <summary>
        /// Gets or sets the current state of the provider.
        /// </summary>
        public ProviderState State
        {
            get => _state;
            protected set
            {
                var oldState = _state;
                var newState = value;
                if (oldState != newState)
                {
                    _state = newState;
                    StateChanged?.Invoke(this, new StateChangedEventArgs(oldState, newState));
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>
        /// Gets or sets the service client instance for making Graph calls.
        /// </summary>
        public GraphServiceClient Graph { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProvider"/> class.
        /// </summary>
        public BaseProvider()
        {
            _state = ProviderState.Loading;
        }

        /// <inheritdoc />
        public abstract Task LoginAsync();

        /// <inheritdoc />
        public abstract Task LogoutAsync();

        /// <inheritdoc />
        public abstract Task AuthenticateRequestAsync(HttpRequestMessage request);
    }
}
