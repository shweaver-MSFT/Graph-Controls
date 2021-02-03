// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI.Core;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// Visual representation of a single To Do task.
    /// </summary>
    public partial class TaskItem : BaseGraphControl
    {
        /// <summary>
        /// Various custom states for the TaskItem control.
        /// </summary>
        private enum TaskItemStates
        {
            /// <summary>
            /// The component has not yet begun loading data.
            /// </summary>
            Unloaded,

            /// <summary>
            /// An indeterminate state while the task data is loading data.
            /// </summary>
            Loading,

            /// <summary>
            /// The component has completed loading data.
            /// </summary>
            Loaded,

            /// <summary>
            /// The task has been deleted from the Graph.
            /// </summary>
            Deleted,

            /// <summary>
            /// Default state with task title and metadata visible.
            /// </summary>
            Normal,

            /// <summary>
            /// Input state where the title and metadata can be edited.
            /// </summary>
            Editing,

            /// <summary>
            /// The task has been completed.
            /// </summary>
            Completed,

            /// <summary>
            /// The task is not yet completed.
            /// </summary>
            Uncompleted,
        }

        private bool GoToVisualState(TaskItemStates state, bool useTransitions = false)
        {
            return GoToVisualState(state.ToString(), useTransitions);
        }

        private async void UpdateVisualState()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Loading states
                if (IsDeleted)
                {
                    GoToVisualState(TaskItemStates.Deleted);
                }
                else if (IsLoading)
                {
                    GoToVisualState(TaskItemStates.Loading);
                }
                else
                {
                    GoToVisualState(TaskItemStates.Loaded);
                }

                // Common states
                if (IsEditModeEnabled)
                {
                    GoToVisualState(TaskItemStates.Editing);
                }
                else
                {
                    GoToVisualState(TaskItemStates.Normal);
                }

                // Completion states
                if (IsCompleted)
                {
                    GoToVisualState(TaskItemStates.Completed);
                }
                else
                {
                    GoToVisualState(TaskItemStates.Uncompleted);
                }
            });
        }
    }
}