// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Controls.Data;
using Microsoft.Toolkit.Graph.Controls.Extensions;
using Microsoft.Toolkit.Graph.Providers;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// A visualization of a single Todo task item.
    /// </summary>
    [TemplatePart(Name = TaskTitleInputTextBoxPart, Type = typeof(TextBox))]
    [TemplatePart(Name = TaskStatusCheckBoxPart, Type = typeof(CheckBox))]
    public partial class TaskItem : BaseGraphControl
    {
        private const string TaskTitleInputTextBoxPart = "PART_TaskTitleInputTextBox";
        private const string TaskStatusCheckBoxPart = "PART_TaskStatusCheckBox";

        private TextBox _taskTitleInputTextBox = null;
        private CheckBox _taskStatusCheckBox = null;

        private bool _isDeleted = false;
        private bool _isLoading = false;

        /// <summary>
        /// Gets or sets a value indicating whether the underlying task has been deleted.
        /// </summary>
        protected bool IsDeleted
        {
            get => _isDeleted;
            set => Set(ref _isDeleted, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is loading.
        /// </summary>
        protected bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private void Set<T>(ref T field, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskItem"/> class.
        /// </summary>
        public TaskItem()
        {
            this.DefaultStyleKey = typeof(TaskItem);
            ProviderManager.Instance.GlobalProvider.StateChanged += (s, e) => UpdateVisualState();
            ContextRequested += OnContextRequested;
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_taskTitleInputTextBox != null)
            {
                _taskTitleInputTextBox.KeyUp -= TaskTitleInputTextBox_KeyUp;
            }

            _taskTitleInputTextBox = GetTemplateChild(TaskTitleInputTextBoxPart) as TextBox;

            if (_taskTitleInputTextBox != null)
            {
                _taskTitleInputTextBox.KeyUp += TaskTitleInputTextBox_KeyUp;
            }

            if (_taskStatusCheckBox != null)
            {
                _taskStatusCheckBox.Checked -= TaskStatusCheckBox_Checked;
                _taskStatusCheckBox.Unchecked -= TaskStatusCheckBox_Unchecked;
            }

            _taskStatusCheckBox = GetTemplateChild(TaskStatusCheckBoxPart) as CheckBox;

            if (_taskStatusCheckBox != null)
            {
                _taskStatusCheckBox.Checked += TaskStatusCheckBox_Checked;
                _taskStatusCheckBox.Unchecked += TaskStatusCheckBox_Unchecked;
            }
        }

        private void OnContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            if (TaskDetails == null)
            {
                return;
            }

            var contextMenu = new MenuFlyout();
            contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Delete task", Command = new DelegateCommand(DeleteTask) });

            if (IsEditModeEnabled)
            {
                contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Save edits", Command = new DelegateCommand(SaveEditsAsync) });
            }
            else
            {
                contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Rename task", Command = new DelegateCommand(ShowEditMode) });
                contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Open in To Do", Command = new DelegateCommand(OpenInToDoApp) });

                switch (TaskDetails.Status)
                {
                    case Microsoft.Graph.TaskStatus.Completed:
                        contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Unmark as completed", Command = new DelegateCommand(UnmarkAsCompleted) });
                        break;

                    default:
                        contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Mark as completed", Command = new DelegateCommand(MarkAsCompleted) });
                        break;
                }
            }

            if (args.TryGetPosition(sender, out Point point))
            {
                contextMenu.ShowAt(this, new FlyoutShowOptions() { Position = point });
            }
            else
            {
                contextMenu.ShowAt(this);
            }
        }

        private void TaskStatusCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            IsCompleted = true;
        }

        private void TaskStatusCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            IsCompleted = false;
        }

        private void TaskTitleInputTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                SaveEditsAsync();
            }
        }

        /// <inheritdoc/>
        protected override async Task LoadDataAsync()
        {
            if (IsLoading)
            {
                return;
            }

            IsLoading = true;

            try
            {
                if (TaskDetails == null && !string.IsNullOrWhiteSpace(TaskListId) && !string.IsNullOrWhiteSpace(TaskId))
                {
                    TaskDetails = await TodoTaskDataSource.GetTaskAsync(TaskListId, TaskId);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <inheritdoc/>
        protected override Task ClearDataAsync()
        {
            TaskDetails = null;
            IsEditModeEnabled = false;
            return Task.CompletedTask;
        }

        private void MarkAsCompleted()
        {
            IsCompleted = true;
        }

        private void UnmarkAsCompleted()
        {
            IsCompleted = false;
        }

        private void ShowEditMode()
        {
            IsEditModeEnabled = true;
        }

        private void HideEditMode()
        {
            IsEditModeEnabled = false;
        }

        private async void DeleteTask()
        {
            IsLoading = true;

            try
            {
                await TodoTaskDataSource.DeleteTaskAsync(TaskListId, TaskId);
                IsDeleted = true;
                FireTaskDeletedEvent();
            }
            catch (Exception e)
            {
                // TODO: Handle failure to delete task.
                System.Diagnostics.Debug.WriteLine("Failed to delete task: " + e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenInToDoApp()
        {
            System.Diagnostics.Debug.WriteLine("Open the task in todo app");
        }

        private async void SaveEditsAsync()
        {
            if (!IsEditModeEnabled)
            {
                // Only save when in the appropriate mode.
                return;
            }

            var inputText = _taskTitleInputTextBox.Text;
            if (string.IsNullOrWhiteSpace(inputText))
            {
                // Don't save if the title input field is empty.
                return;
            }

            IsLoading = true;

            var taskDetailsForSave = new TodoTask()
            {
                Id = TaskDetails.Id,
                Title = inputText.Trim(),
            };

            try
            {
                if (TaskDetails.IsNew())
                {
                    TaskDetails = await TodoTaskDataSource.AddTaskAsync(TaskListId, taskDetailsForSave);
                }
                else
                {
                    var updatedTask = await TodoTaskDataSource.UpdateTaskAsync(TaskListId, taskDetailsForSave);
                    TaskDetails.Title = updatedTask.Title;
                }

                // Sync up with the response data
                TaskId = TaskDetails.Id;
                TaskTitle = TaskDetails.Title;
                IsCompleted = TaskDetails.IsCompleted();
            }
            catch (Exception e)
            {
                // TODO: Handle failure to save modified task details
                System.Diagnostics.Debug.WriteLine("Failed to save edits: " + e.Message);
                return;
            }
            finally
            {
                IsLoading = false;
            }

            HideEditMode();
            FireTaskDetailsChangedEvent();
        }

        private class DelegateCommand<T> : ICommand
        {
            private Action<T> _executeAction;

            public event EventHandler CanExecuteChanged = null;

            public DelegateCommand(Action<T> executeAction)
            {
                _executeAction = executeAction;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _executeAction((T)parameter);
            }
        }

        private class DelegateCommand : ICommand
        {
            private Action _executeAction;

            public event EventHandler CanExecuteChanged = null;

            public DelegateCommand(Action executeAction)
            {
                _executeAction = executeAction;
            }

            public bool CanExecute(object parameter = null)
            {
                return true;
            }

            public void Execute(object parameter = null)
            {
                _executeAction();
            }
        }
    }
}
