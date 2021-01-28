using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Graph.Providers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    ///  A base class for building Graph powered controls.
    /// </summary>
    public abstract class BaseGraphControl : Control
    {
        /// <summary>
        /// An list of common states that Graph based controls should support at a minimum.
        /// </summary>
        public enum CommonStates
        {
            /// <summary>
            /// The control is in a indeterminate state
            /// </summary>
            Loading,

            /// <summary>
            /// The control has Graph context and can behave properly
            /// </summary>
            SignedIn,

            /// <summary>
            /// The control does not have Graph context and cannot load any data.
            /// </summary>
            SignedOut,

            /// <summary>
            /// There was an error loading the control.
            /// </summary>
            Error,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphControl"/> class.
        /// </summary>
        public BaseGraphControl()
        {
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
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                GoToVisualState(CommonStates.Loading);

                try
                {
                    var provider = ProviderManager.Instance.GlobalProvider;
                    if (provider == null)
                    {
                        await ClearDataAsync();
                        GoToVisualState(CommonStates.SignedOut);
                        return;
                    }

                    switch (provider.State)
                    {
                        case ProviderState.SignedIn:
                            await LoadDataAsync();
                            GoToVisualState(CommonStates.SignedIn);
                            break;
                        case ProviderState.SignedOut:
                            await ClearDataAsync();
                            GoToVisualState(CommonStates.SignedOut);
                            break;
                        case ProviderState.Loading:
                            GoToVisualState(CommonStates.Loading);
                            break;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                    IsEnabled = false;
                    GoToVisualState(CommonStates.Error);
                }
            });
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="useTransitions"></param>
        /// <returns></returns>
        protected bool GoToVisualState(CommonStates state, bool useTransitions = false)
        {
            return GoToVisualState(state.ToString(), useTransitions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="useTransitions"></param>
        /// <returns></returns>
        protected bool GoToVisualState(string state, bool useTransitions = false)
        {
            return VisualStateManager.GoToState(this, state, useTransitions);
        }
    }
}
