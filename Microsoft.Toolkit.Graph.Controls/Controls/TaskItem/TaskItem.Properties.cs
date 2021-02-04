// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Controls.Data;
using Microsoft.Toolkit.Graph.Controls.Extensions;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// Visual representation of a single To Do task.
    /// </summary>
    public partial class TaskItem : BaseGraphControl
    {
        /// <summary>
        /// Gets or sets the Task id property value.
        /// </summary>
        public string TaskId
        {
            get { return (string)GetValue(TaskIdProperty); }
            set { SetValue(TaskIdProperty, value); }
        }

        /// <summary>
        /// Todo task id value.
        /// </summary>
        public static readonly DependencyProperty TaskIdProperty =
            DependencyProperty.Register(nameof(TaskId), typeof(string), typeof(TaskItem), new PropertyMetadata(null));

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

        /// <summary>
        /// Gets or sets the Task title property value.
        /// </summary>
        public string TaskTitle
        {
            get { return (string)GetValue(TaskTitleProperty); }
            set { SetValue(TaskTitleProperty, value); }
        }

        /// <summary>
        /// Todo task title value.
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
                var newTaskDetails = (TodoTask)e.NewValue;
                if (newTaskDetails == null)
                {
                    taskItem.IsCompleted = false;
                    taskItem.IsEditModeEnabled = false;
                    taskItem.TaskTitle = null;
                    taskItem.TaskId = null;
                }
                else
                {
                    taskItem.IsCompleted = taskItem.TaskDetails.IsCompleted();
                    taskItem.TaskTitle = taskItem.TaskDetails.Title;
                    taskItem.TaskId = taskItem.TaskDetails.Id;
                }

                taskItem.UpdateVisualState();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the task status is completed.
        /// </summary>
        public bool IsCompleted
        {
            get { return (bool)GetValue(IsCompletedProperty); }
            set { SetValue(IsCompletedProperty, value); }
        }

        /// <summary>
        /// IsCompleted property.
        /// </summary>
        public static readonly DependencyProperty IsCompletedProperty =
            DependencyProperty.Register(nameof(IsCompleted), typeof(bool), typeof(TaskItem), new PropertyMetadata(false, OnIsCompletedChanged));

        private static async void OnIsCompletedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            await d.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (d is TaskItem taskItem)
                {
                    var task = taskItem.TaskDetails;
                    if (task == null)
                    {
                        return;
                    }

                    taskItem.IsLoading = true;

                    var newIsCompleted = (bool)e.NewValue;
                    if (!task.IsNew() && task.IsCompleted() != newIsCompleted)
                    {
                        var newStatus = newIsCompleted ? TaskStatus.Completed : TaskStatus.NotStarted;
                        var taskForUpdate = new TodoTask()
                        {
                            Id = task.Id,
                            Status = newStatus,
                        };

                        try
                        {
                            var updatedTask = await TodoTaskDataSource.UpdateTaskAsync(taskItem.TaskListId, taskForUpdate);
                            taskItem.TaskDetails.Status = updatedTask.Status;

                            if (taskItem.TaskDetails.IsCompleted())
                            {
                                taskItem.FireTaskCompletedEvent();
                            }
                            else
                            {
                                taskItem.FireTaskUncompletedEvent();
                            }
                        }
                        catch
                        {
                            // Restore previous value if not successful.
                            taskItem.IsCompleted = (bool)e.OldValue;
                        }
                    }

                    taskItem.IsLoading = false;
                }
            });
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
                        taskItem._taskTitleInputTextBox.Text = taskItem.TaskTitle ?? string.Empty;
                        taskItem._taskTitleInputTextBox.Focus(FocusState.Programmatic);
                    }
                    else
                    {
                        taskItem._taskTitleInputTextBox.Text = string.Empty;
                        taskItem.Focus(FocusState.Programmatic);
                    }

                    taskItem.UpdateVisualState();
                }
            });
        }
    }
}
