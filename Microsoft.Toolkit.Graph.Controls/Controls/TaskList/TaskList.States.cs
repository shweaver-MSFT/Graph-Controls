// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Graph.Providers;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TaskList
    {
        /// <summary>
        /// Custom visual states for the TaskList control.
        /// </summary>
        protected enum TaskListStates
        {
            /// <summary>
            /// An indeterminate state while the provider is not signed in.
            /// </summary>
            Unloaded,

            /// <summary>
            /// An state to use while data is loading.
            /// </summary>
            Loading,

            /// <summary>
            /// The usable state with data available.
            /// </summary>
            Loaded,

            /// <summary>
            /// A collapsed state where the content is hidden from view.
            /// </summary>
            Collapsed,
        }

        private void UpdateVisualState()
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider.State == ProviderState.SignedIn)
            {
                if (IsLoading)
                {
                    GoToVisualState(TaskListStates.Loading, true);
                }
                else
                {
                    GoToVisualState(TaskListStates.Loaded, true);
                }
            }
            else
            {
                GoToVisualState(TaskListStates.Unloaded, true);
            }

            if (IsContentCollapsed)
            {
                GoToVisualState(TaskListStates.Collapsed, true);
            }
        }

        private bool GoToVisualState(TaskListStates state, bool useTransitions = false)
        {
            return GoToVisualState(state.ToString(), useTransitions);
        }
    }
}
