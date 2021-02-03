// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// Visual representation of a single To Do task.
    /// </summary>
    public partial class TaskItem : BaseGraphControl
    {
        /// <summary>
        /// An event that fires when the task has been marked completed.
        /// </summary>
        public event RoutedEventHandler TaskCompleted;

        /// <summary>
        /// An event that fires when the task has been unmarked as completed.
        /// </summary>
        public event RoutedEventHandler TaskUncompleted;

        /// <summary>
        /// An event that fires when the task is deleted.
        /// </summary>
        public event RoutedEventHandler TaskDeleted;

        /// <summary>
        /// An event to fire when the underlying todo task has changed.
        /// </summary>
        public event RoutedEventHandler TaskDetailsChanged;

        private void FireTaskCompletedEvent()
        {
            TaskCompleted?.Invoke(this, new RoutedEventArgs());
        }

        private void FireTaskUncompletedEvent()
        {
            TaskUncompleted?.Invoke(this, new RoutedEventArgs());
        }

        private void FireTaskDeletedEvent()
        {
            TaskDeleted?.Invoke(this, new RoutedEventArgs());
        }

        private void FireTaskDetailsChangedEvent()
        {
            TaskDetailsChanged?.Invoke(this, new RoutedEventArgs());
        }
    }
}