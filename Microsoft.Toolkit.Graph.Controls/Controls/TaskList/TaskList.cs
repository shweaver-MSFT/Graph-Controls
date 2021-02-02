// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// foo
    /// </summary>
    [TemplatePart(Name = AddTaskButtonPart, Type = typeof(ButtonBase))]
    [TemplatePart(Name = OverflowMenuButtonPart, Type = typeof(ButtonBase))]
    [TemplatePart(Name = AvailableTasksListViewPart, Type = typeof(ListView))]
    [TemplatePart(Name = CompletedTasksListViewPart, Type = typeof(ListView))]
    public partial class TaskList : BaseGraphControl
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

        private const string AddTaskButtonPart = "PART_AddTaskButton";
        private const string OverflowMenuButtonPart = "PART_OverflowMenuButton";
        private const string AvailableTasksListViewPart = "PART_AvailableTasksListView";
        private const string CompletedTasksListViewPart = "PART_CompletedTasksListView";

        private Button _addTaskButton;
        private Button _overflowMenuButton;
        private ListView _availableTasksListView;
        private ListView _completedTasksListView;

        private bool _isLoading;

        /// <summary>
        /// Gets or sets a value indicating whether the component is loading.
        /// </summary>
        protected bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskList"/> class.
        /// </summary>
        public TaskList()
        {
            DefaultStyleKey = typeof(TaskList);
            IsLoading = false;
            ProviderManager.Instance.GlobalProvider.StateChanged += (s, e) => UpdateVisualState();
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_addTaskButton != null)
            {
                _addTaskButton.Click -= AddTaskButton_Click;
            }

            _addTaskButton = GetTemplateChild(AddTaskButtonPart) as Button;

            if (_addTaskButton != null)
            {
                _addTaskButton.Click += AddTaskButton_Click;
            }

            if (_overflowMenuButton != null)
            {
                _overflowMenuButton.Click -= OverflowMenuButton_Click;
            }

            _overflowMenuButton = GetTemplateChild(OverflowMenuButtonPart) as Button;

            if (_overflowMenuButton != null)
            {
                _overflowMenuButton.Click += OverflowMenuButton_Click;
            }

            if (_availableTasksListView != null)
            {
                _availableTasksListView.ItemClick -= AvailableTasksListView_ItemClick;
                _availableTasksListView.ContextRequested -= AvailableTasksListView_ContextRequested;
            }

            _availableTasksListView = GetTemplateChild(AvailableTasksListViewPart) as ListView;

            if (_availableTasksListView != null)
            {
                _availableTasksListView.ItemClick += AvailableTasksListView_ItemClick;
                _availableTasksListView.ContextRequested += AvailableTasksListView_ContextRequested;
            }

            if (_completedTasksListView != null)
            {
                _completedTasksListView.ContextRequested -= CompletedTasksListView_ContextRequested;
            }

            _completedTasksListView = GetTemplateChild(CompletedTasksListViewPart) as ListView;

            if (_completedTasksListView != null)
            {
                _completedTasksListView.ContextRequested += CompletedTasksListView_ContextRequested;
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

            TaskLists = await TaskItemDataSource.GetMyTaskListsAsync();

            // Apply the Graph data
            if (TaskLists != null && TaskLists.Count() > 0)
            {
                if (TaskListId != null)
                {
                    // Check for a specified default list, and set that if possible.
                    var defaultTaskList = TaskLists.Where((l) => l.Id == TaskListId).FirstOrDefault();
                    if (defaultTaskList != default)
                    {
                        SelectedTaskListIndex = TaskLists.IndexOf(defaultTaskList);
                    }
                }

                SelectedTaskList = TaskLists[SelectedTaskListIndex];
                TaskListId = SelectedTaskList.Id;
            }

            IsLoading = false;
            UpdateVisualState();
        }

        /// <inheritdoc/>
        protected override async Task ClearDataAsync()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SelectedTaskList = null;
                TaskLists.Clear();
                CompletedTasks.Clear();
                AvailableTasks.Clear();
                SelectedTaskListIndex = 0;
            });
        }

        /// <summary>
        /// Update the visual state based upon the current conditions.
        /// </summary>
        protected void UpdateVisualState()
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

        #region Element event methods
        private void TaskItemData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TaskItemData updatedTaskItemData && e.PropertyName == nameof(TaskItemData.TaskDetails))
            {
                switch (updatedTaskItemData.TaskDetails.Status)
                {
                    case Microsoft.Graph.TaskStatus.Completed:
                        foreach (var taskItemData in AvailableTasks)
                        {
                            if (updatedTaskItemData.TaskDetails.Id == taskItemData.TaskDetails.Id)
                            {
                                MarkTaskAsCompleted(updatedTaskItemData);
                                break;
                            }
                        }

                        break;
                    default:
                        foreach (var taskItemData in CompletedTasks)
                        {
                            if (updatedTaskItemData.TaskDetails.Id == taskItemData.TaskDetails.Id)
                            {
                                UnmarkTaskAsCompleted(updatedTaskItemData);
                                break;
                            }
                        }

                        break;
                }
            }
        }

        private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (_availableTasksListView != null &&
                    _availableTasksListView.Items.Count > 0 &&
                    _availableTasksListView.Items[0] is TaskItemData taskData &&
                    taskData.IsNew)
                {
                    // A new item is already ready for input.
                    return;
                }

                AvailableTasks.Insert(0, new TaskItemData(new TodoTask(), TaskListId));
            });
        }

        private void OverflowMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOverflowContextMenu(sender as Button);
        }

        private void AvailableTasksListView_ItemClick(object sender, RoutedEventArgs e)
        {
            var task = (e.OriginalSource as FrameworkElement).DataContext;
            System.Diagnostics.Debug.WriteLine("Item clicked");
        }

        private void AvailableTasksListView_ContextRequested(object sender, ContextRequestedEventArgs e)
        {
            var taskItem = GetChildTaskItem(e.OriginalSource);
            if (taskItem != null)
            {
                ShowTaskItemContextMenu(taskItem);
            }
        }

        private void CompletedTasksListView_ContextRequested(object sender, ContextRequestedEventArgs e)
        {
            var taskItem = GetChildTaskItem(e.OriginalSource);
            if (taskItem != null)
            {
                ShowTaskItemContextMenu(taskItem);
            }
        }
        #endregion

        #region TaskItemContextMenu
        private void ShowTaskItemContextMenu(TaskItem taskItem)
        {
            var task = taskItem.TaskDetails;
            var taskItemData = taskItem.DataContext as TaskItemData;

            var contextMenu = new MenuFlyout();
            contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Delete task", Command = new DelegateCommand<TaskItemData>(DeleteTask), CommandParameter = taskItemData });

            if (!taskItem.IsEditModeEnabled)
            {
                contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Rename task", Command = new DelegateCommand<TaskItem>(ShowTaskEditMode), CommandParameter = taskItem });
                contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Open in To Do", Command = new DelegateCommand<TaskItem>(OpenTaskInToDoApp), CommandParameter = taskItem });

                switch (task.Status)
                {
                    case Microsoft.Graph.TaskStatus.Completed:
                        contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Unmark as completed", Command = new DelegateCommand<TaskItemData>(UnmarkTaskAsCompleted), CommandParameter = taskItemData });
                        break;

                    default:
                        contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Select for focus session", Command = new DelegateCommand<TaskItem>(SelectTaskForFocusSession), CommandParameter = taskItem });
                        contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Mark as completed", Command = new DelegateCommand<TaskItemData>(MarkTaskAsCompleted), CommandParameter = taskItemData });
                        break;
                }
            }

            contextMenu.ShowAt(taskItem);
        }

        private void ShowTaskEditMode(TaskItem taskItem)
        {
            taskItem.IsEditModeEnabled = true;
        }

        private async void DeleteTask(TaskItemData taskData)
        {
            // "New" tasks aren't actually saved to the Graph yet.
            if (!taskData.IsNew)
            {
                try
                {
                    // Delete task from Graph.
                    await TaskItemDataSource.DeleteTaskAsync(taskData.TaskId, taskData.TaskListId);
                }
                catch
                {
                    // TODO: Handle failure to delete the task.
                    return;
                }
            }

            // Remove the task fromt he appropriate list.
            if (taskData.IsCompleted)
            {
                CompletedTasks.RemoveById(taskData.TaskId);
            }
            else
            {
                AvailableTasks.RemoveById(taskData.TaskId);
            }
        }

        private void OpenTaskInToDoApp(TaskItem taskItem)
        {
            System.Diagnostics.Debug.WriteLine("Open task in To Do");
        }

        private void SelectTaskForFocusSession(TaskItem taskItem)
        {
            System.Diagnostics.Debug.WriteLine("Select task for focus session");

            var taskData = taskItem.DataContext as TaskItemData;
            if (taskData.IsNew)
            {
                var itemIndex = _availableTasksListView.IndexFromContainer(taskItem);
                _availableTasksListView.SelectRange(new ItemIndexRange(itemIndex, 1));
            }
        }

        private async void MarkTaskAsCompleted(TaskItemData taskItemData)
        {
            System.Diagnostics.Debug.WriteLine("Mark task as completed");

            var success = await taskItemData.MarkAsCompletedAsync();
            if (!success)
            {
                // TODO: Handle failure to mark task as completed.
                return;
            }

            AvailableTasks.RemoveById(taskItemData.TaskId);
            CompletedTasks.Add(taskItemData);
        }

        private async void UnmarkTaskAsCompleted(TaskItemData taskItemData)
        {
            System.Diagnostics.Debug.WriteLine("Unmark task as completed");

            var success = await taskItemData.UnmarkAsCompletedAsync();
            if (!success)
            {
                // TODO: Handle failure to unmark task as completed.
                return;
            }

            CompletedTasks.RemoveById(taskItemData.TaskId);
            AvailableTasks.Add(taskItemData);
        }
        #endregion

        #region OverflowContextMenu
        private void ShowOverflowContextMenu(FrameworkElement target)
        {
            var contextMenu = new MenuFlyout();
            if (IsContentCollapsed)
            {
                contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Show Microsoft To Do", Command = new DelegateCommand(ShowContent) });
            }
            else
            {
                contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Hide Microsoft To Do", Command = new DelegateCommand(CollapseContent) });
            }

            switch (ProviderManager.Instance.GlobalProvider.State)
            {
                case ProviderState.SignedOut:
                    contextMenu.Items.Add(new MenuFlyoutItem() { Text = "View Settings", Command = new DelegateCommand(LaunchToDoAppSettings) });
                    break;
                case ProviderState.SignedIn:
                    contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Launch Microsoft To Do", Command = new DelegateCommand(LaunchToDoApp) });
                    break;
            }

            contextMenu.ShowAt(target);
        }

        private void LaunchToDoApp()
        {
            // TODO: Launch the ToDo app.
            System.Diagnostics.Debug.WriteLine("Launch To Do app");
        }

        private void LaunchToDoAppSettings()
        {
            System.Diagnostics.Debug.WriteLine("Launch To Do app settings");
        }

        private void ShowContent()
        {
            IsContentCollapsed = false;
        }

        private void CollapseContent()
        {
            IsContentCollapsed = true;
        }
        #endregion

        /// <summary>
        /// Helper function for finding the TaskItem element in the VisualTree.
        /// May return null if the element cannot be found.
        /// </summary>
        /// <param name="originalSource"></param>
        /// <returns></returns>
        private TaskItem GetChildTaskItem(object originalSource)
        {
            var sourceElement = originalSource as FrameworkElement;
            if (sourceElement is TaskItem taskItem)
            {
                return taskItem;
            }

            if (sourceElement is ListViewItemPresenter)
            {
                var itemPresenter = sourceElement as ListViewItemPresenter;
                return itemPresenter.FindDescendant<TaskItem>();
            }

            DependencyObject temp = sourceElement;
            while (temp.GetType() != typeof(TaskItem))
            {
                temp = VisualTreeHelper.GetParent(temp);
                if (temp == null)
                {
                    return null;
                }
            }

            return temp as TaskItem;
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
