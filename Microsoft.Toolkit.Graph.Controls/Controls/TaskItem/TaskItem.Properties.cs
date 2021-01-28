using System;
using Microsoft.Graph;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// foo
    /// </summary>
    public partial class TaskItem
    {
        /// <summary>
        /// Gets or sets the TaskList id property value.
        /// </summary>
        public string TaskListId
        {
            get { return (string)GetValue(TaskListIdProperty); }
            set { SetValue(TaskListIdProperty, value); }
        }

        /// <summary>
        /// Todo task list id value.
        /// </summary>
        public static readonly DependencyProperty TaskListIdProperty =
            DependencyProperty.Register(nameof(TaskListId), typeof(string), typeof(TaskItem), new PropertyMetadata(null));

        // <summary>
        /// Gets or sets the TaskList id property value.
        /// </summary>
        public string TaskTitleInput
        {
            get { return (string)GetValue(TaskTitleInputProperty); }
            set { SetValue(TaskTitleInputProperty, value); }
        }

        /// <summary>
        /// Todo task title input value.
        /// </summary>
        public static readonly DependencyProperty TaskTitleInputProperty =
            DependencyProperty.Register(nameof(TaskTitleInput), typeof(string), typeof(TaskItem), new PropertyMetadata(null));

        // <summary>
        /// Gets or sets the TaskList id property value.
        /// </summary>
        public string TaskTitle
        {
            get { return (string)GetValue(TaskTitleProperty); }
            set { SetValue(TaskTitleProperty, value); }
        }

        /// <summary>
        /// Todo task title input value.
        /// </summary>
        public static readonly DependencyProperty TaskTitleProperty =
            DependencyProperty.Register(nameof(TaskTitle), typeof(string), typeof(TaskItem), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the TaskDetails property value.
        /// </summary>
        public TodoTask TaskDetails
        {
            get { return (TodoTask)GetValue(TaskDetailsProperty); }
            set { SetValue(TaskDetailsProperty, value); }
        }

        /// <summary>
        /// Todo task item metadata.
        /// </summary>
        public static readonly DependencyProperty TaskDetailsProperty =
            DependencyProperty.Register(nameof(TaskDetails), typeof(TodoTask), typeof(TaskItem), new PropertyMetadata(null, OnTaskDetailsChanged));

        private static void OnTaskDetailsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TaskItem taskItem)
            {
                taskItem.IsCompleted = taskItem.TaskDetails?.Status == TaskStatus.Completed;
                taskItem.TaskTitle = taskItem.TaskDetails?.Title;
            }
        }

        public bool IsNew => TaskDetails.CreatedDateTime == null;

        /// <summary>
        /// 
        /// </summary>
        public bool IsCompleted
        {
            get { return (bool)GetValue(IsCompletedProperty); }
            set { SetValue(IsCompletedProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty IsCompletedProperty =
            DependencyProperty.Register(nameof(IsCompleted), typeof(bool), typeof(TaskItem), new PropertyMetadata(false, OnIsCompletedChanged));

        private static async void OnIsCompletedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TaskItem taskItem)
            {
                var taskData = new TaskItemData(taskItem.TaskDetails, taskItem.TaskListId);

                // Don't change status for new tasks
                if (taskData.IsNew)
                {
                    return;
                }

                // Toggle the status as appropriate
                if (taskData.IsCompleted)
                {
                    await taskData.MarkAsCompletedAsync();
                }
                else
                {
                    await taskData.UnmarkAsCompletedAsync();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control is in an editable state.
        /// </summary>
        public bool IsEditModeEnabled
        {
            get { return (bool)GetValue(IsEditModeEnabledProperty); }
            set { SetValue(IsEditModeEnabledProperty, value); }
        }

        /// <summary>
        /// Edit mode enablement property.
        /// </summary>
        public static readonly DependencyProperty IsEditModeEnabledProperty =
            DependencyProperty.Register(nameof(IsEditModeEnabled), typeof(bool), typeof(TaskItem), new PropertyMetadata(false, OnEditModeChanged));

        private static async void OnEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            await d.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (d is TaskItem taskItem && taskItem._taskTitleInputTextBox != null)
                {
                    if (taskItem.IsEditModeEnabled)
                    {
                        taskItem.TaskTitleInput = taskItem.TaskTitle;
                        taskItem._taskTitleInputTextBox.Focus(FocusState.Programmatic);
                    }
                    else
                    {
                        taskItem.TaskTitleInput = string.Empty;
                        taskItem.Focus(FocusState.Programmatic);
                    }
                }
            });
        }
    }
}
