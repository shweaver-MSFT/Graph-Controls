using System;
using Microsoft.Toolkit.Graph.Providers;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    ///  A base class for building Graph powered controls.
    /// </summary>
    public abstract class BaseGraphControl : Control
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseGraphControl"/> class.
        /// </summary>
        public BaseGraphControl()
        {
            ProviderManager.Instance.ProviderUpdated += (sender, args) => Update();
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider != null && provider.State == ProviderState.SignedIn)
            {
                try
                {
                    LoadData();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                    IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Update the data state of the control.
        /// </summary>
        protected void Update()
        {
            try
            {
                var provider = ProviderManager.Instance.GlobalProvider;
                if (provider == null)
                {
                    ClearData();
                    return;
                }

                switch (provider.State)
                {
                    case ProviderState.SignedIn:
                        LoadData();
                        break;
                    case ProviderState.SignedOut:
                        ClearData();
                        break;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                IsEnabled = false;
            }
        }

        /// <summary>
        /// Load data from the Graph and apply the values.
        /// </summary>
        protected virtual void LoadData()
        {
        }

        /// <summary>
        /// Clear any data state and reset default values.
        /// </summary>
        protected virtual void ClearData()
        {
        }
    }
}
