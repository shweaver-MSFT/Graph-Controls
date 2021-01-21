using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Graph.Providers;
using Windows.Foundation.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    ///  A base class for building Graph powered controls.
    /// </summary>
    public abstract class BaseGraphControl : Control
    {
        /// <summary>
        /// Gets or sets a value indicating whether the control is loading and has not established a sign-in state.
        /// </summary>
        public bool IsLoading { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphControl"/> class.
        /// </summary>
        public BaseGraphControl()
        {
            IsLoading = false;
            ProviderManager.Instance.ProviderUpdated += async (s, a) => await UpdateAsync();
        }

        /// <inheritdoc/>
        protected override async void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            await UpdateAsync();
        }

        /// <summary>
        /// Update the data state of the control.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task UpdateAsync()
        {
            IsLoading = true;
            try
            {
                var provider = ProviderManager.Instance.GlobalProvider;
                if (provider == null)
                {
                    await ClearDataAsync();
                    IsLoading = false;
                    return;
                }

                switch (provider.State)
                {
                    case ProviderState.SignedIn:
                        await LoadDataAsync();
                        IsLoading = false;
                        break;
                    case ProviderState.SignedOut:
                        await ClearDataAsync();
                        IsLoading = false;
                        break;
                    case ProviderState.Loading:
                        IsLoading = true;
                        break;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                IsEnabled = false;
                IsLoading = false;
            }
        }

        /// <summary>
        /// Load data from the Graph and apply the values.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clear any data state and reset default values.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task ClearDataAsync()
        {
            return Task.CompletedTask;
        }
    }
}
