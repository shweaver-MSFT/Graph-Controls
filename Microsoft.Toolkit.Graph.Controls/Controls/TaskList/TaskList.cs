// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Controls.Data;
using Microsoft.Toolkit.Graph.Controls.Extensions;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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

            TodoTaskDataSource.TaskUpdated += OnTaskUpdated;
            TodoTaskDataSource.TaskDeleted += OnTaskDeleted;
            TodoTaskDataSource.TaskAdded += OnTaskAdded;


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

            TaskLists = await TodoTaskDataSource.GetMyTaskListsAsync();

            // Apply the Graph data
            if (TaskLists != null && TaskLists.Count() > 0)
            {
                if (TaskListId != null)
                {
                    // Check for a specified default list, and set that if possible.
                    var defaultTaskList = TaskLists.Where((l) => l.Id == TaskListId).FirstOrDefault();
                    if (defaultTaskList != default)
                    {
                        var defaultIndex = TaskLists.IndexOf(defaultTaskList);
                        if (SelectedTaskListIndex == defaultIndex)
                        {
                            // If the SelectedTaskIndex isn't changed, we need to manually trigger the tasks to load.
                            LoadTasks();
                        }
                        else
                        {
                            SelectedTaskListIndex = defaultIndex;
                        }
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
                TaskLists = new ObservableCollection<TodoTaskList>();
                CompletedTasks = new ObservableCollection<TaskDataModel>();
                AvailableTasks = new ObservableCollection<TaskDataModel>();
                SelectedTaskList = null;
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

        private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (AvailableTasks != null &&
                    AvailableTasks.Count > 0 &&
                    AvailableTasks.First() is TaskDataModel taskModel &&
                    taskModel.Task.IsNew())
                {
                    // A new item is already ready for input.
                    return;
                }

                AvailableTasks.Insert(0, new TaskDataModel(SelectedTaskList.Id, new TodoTask()));
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
                System.Diagnostics.Debug.WriteLine("AvailableTasksListView_ContextRequested");
            }
        }

        private void CompletedTasksListView_ContextRequested(object sender, ContextRequestedEventArgs e)
        {
            var taskItem = GetChildTaskItem(e.OriginalSource);
            if (taskItem != null)
            {
                System.Diagnostics.Debug.WriteLine("CompletedTasksListView_ContextRequested");
            }
        }


        private void OnTaskUpdated(object sender, TodoTask task)
        {
            var taskListId = (string)sender;

            if (task.IsCompleted())
            {
                foreach (var taskModel in AvailableTasks)
                {
                    if (taskModel.Task.Id == task.Id)
                    {
                        AvailableTasks.Remove(taskModel);
                        CompletedTasks.Add(new TaskDataModel(taskListId, task));
                        break;
                    }
                }
            }
            else
            {
                foreach (var taskModel in CompletedTasks)
                {
                    if (taskModel.Task.Id == task.Id)
                    {
                        CompletedTasks.Remove(taskModel);
                        AvailableTasks.Add(new TaskDataModel(taskListId, task));
                        break;
                    }
                }
            }
        }

        private void OnTaskDeleted(object sender, string taskId)
        {
            // Find the task by id and remove it.
            foreach (var taskModel in AvailableTasks)
            {
                if (taskModel.Task.Id == taskId)
                {
                    AvailableTasks.Remove(taskModel);
                    return;
                }
            }

            foreach (var taskModel in CompletedTasks)
            {
                if (taskModel.Task.Id == taskId)
                {
                    CompletedTasks.Remove(taskModel);
                    break;
                }
            }
        }

        private void OnTaskAdded(object sender, TodoTask task)
        {
            var taskListId = (string)sender;
            var newTaskModel = new TaskDataModel(taskListId, task);
            if (task.IsCompleted())
            {
                for (var i = 0; i < CompletedTasks.Count; i++)
                {
                    if (CompletedTasks[i].Task.IsNew())
                    {
                        CompletedTasks.RemoveAt(i);
                        break;
                    }
                }

                CompletedTasks.Insert(0, newTaskModel);
            }
            else
            {
                for (var i = 0; i < AvailableTasks.Count; i++)
                {
                    if (AvailableTasks[i].Task.IsNew())
                    {
                        AvailableTasks.RemoveAt(i);
                        break;
                    }
                }

                AvailableTasks.Insert(0, newTaskModel);
            }
        }

        private async void LoadTasks()
        {
            IsLoading = true;

            CompletedTasks = new ObservableCollection<TaskDataModel>();
            AvailableTasks = new ObservableCollection<TaskDataModel>();

            int taskListIndex = SelectedTaskListIndex;
            if (taskListIndex == -1 || TaskLists.Count == 0 || taskListIndex > TaskLists.Count)
            {
                IsLoading = false;
                return;
            }

            SelectedTaskList = TaskLists[taskListIndex];

            try
            {
                var taskListId = SelectedTaskList.Id;
                var tasks = await TodoTaskDataSource.GetTasksAsync(taskListId);

                foreach (var task in tasks)
                {
                    var taskModel = new TaskDataModel(taskListId, task);

                    if (task.IsCompleted())
                    {
                        CompletedTasks.Add(taskModel);
                    }
                    else
                    {
                        AvailableTasks.Add(taskModel);
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: Handle error to retrieve Tasks
                System.Diagnostics.Debug.WriteLine("Failed to load tasks: " + e.Message);
                GoToErrorState();
                return;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowTaskItemContextMenu(TaskItem taskItem)
        {
            //var task = taskItem.TaskDetails;

            //if (!taskItem.IsEditModeEnabled && !taskItem.IsCompleted)
            //{
            //    var contextMenu = new MenuFlyout();
            //    contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Select for focus session", Command = new DelegateCommand<TaskItem>(SelectTaskForFocusSession), CommandParameter = taskItem });
            //    contextMenu.ShowAt(taskItem);
            //}
        }

        //private void SelectTaskForFocusSession(TaskItem taskItem)
        //{
        //    System.Diagnostics.Debug.WriteLine("Select task for focus session");

        //    var taskData = taskItem.DataContext as TaskItemData;
        //    if (taskData.IsNew)
        //    {
        //        var itemIndex = _availableTasksListView.IndexFromContainer(taskItem);
        //        _availableTasksListView.SelectRange(new ItemIndexRange(itemIndex, 1));
        //    }
        //}


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
