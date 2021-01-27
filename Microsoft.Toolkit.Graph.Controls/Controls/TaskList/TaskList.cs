using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Controls.Extensions;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
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
        private const string AddTaskButtonPart = "PART_AddTaskButton";
        private const string OverflowMenuButtonPart = "PART_OverflowMenuButton";
        private const string AvailableTasksListViewPart = "PART_AvailableTasksListView";
        private const string CompletedTasksListViewPart = "PART_CompletedTasksListView";

        private Button _addTaskButton;
        private Button _overflowMenuButton;
        private ListView _availableTasksListView;
        private ListView _completedTasksListView;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskList"/> class.
        /// </summary>
        public TaskList()
        {
            this.DefaultStyleKey = typeof(TaskList);
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            GoToVisualState(CommonStates.Loading);

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
            GoToVisualState(CommonStates.Loading);

            var graph = ProviderManager.Instance.GlobalProvider.Graph;
            var taskListsPage = await graph.Me.Todo.Lists.Request().GetAsync();
            TaskLists = taskListsPage.CurrentPage;

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

                var tasks = SelectedTaskList.Tasks;
                if (tasks != null && tasks.Count() > 0)
                {
                    TaskListId = SelectedTaskList.Id;

                    foreach (TodoTask task in tasks)
                    {
                        var taskData = new TaskItemData(task, TaskListId);

                        switch (task.Status)
                        {
                            case Microsoft.Graph.TaskStatus.Completed:
                                CompletedTasks.Add(taskData);
                                break;
                            default:
                                AvailableTasks.Add(taskData);
                                break;
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task ClearDataAsync()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SelectedTaskList = null;
                TaskLists.Clear();
                CompletedTasks.Clear();
                AvailableTasks.Clear();
                SelectedTaskListIndex = 0;
            });
        }

        private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (_availableTasksListView != null &&
                    _availableTasksListView.Items.Count > 0 &&
                    _availableTasksListView.Items[0] is TaskItemData taskData &&
                    taskData.TaskDetails.IsNew())
                {
                    // A new item is already ready for input.
                    return;
                }

                AvailableTasks.Insert(0, new TaskItemData(new TodoTask(), TaskListId));
            });
        }

        private void OverflowMenuButton_Click(object sender, RoutedEventArgs e)
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

            contextMenu.ShowAt(sender as Button);
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

        private void ShowTaskItemContextMenu(TaskItem taskItem)
        {
            var task = taskItem.TaskDetails;

            var contextMenu = new MenuFlyout();
            contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Delete task", Command = new DelegateCommand<TaskItem>(DeleteTask), CommandParameter = taskItem });

            if (!taskItem.IsEditModeEnabled)
            {
                contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Rename task", Command = new DelegateCommand<TaskItem>(RenameTask), CommandParameter = taskItem });
                contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Open in To Do", Command = new DelegateCommand<TaskItem>(OpenTaskInToDoApp), CommandParameter = taskItem });

                switch (task.Status)
                {
                    case Microsoft.Graph.TaskStatus.Completed:
                        contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Unmark as completed", Command = new DelegateCommand<TaskItem>(UnmarkTaskAsCompleted), CommandParameter = taskItem });
                        break;

                    default:
                        contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Select for focus session", Command = new DelegateCommand<TaskItem>(SelectTaskForFocusSession), CommandParameter = taskItem });
                        contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Mark as completed", Command = new DelegateCommand<TaskItem>(MarkTaskAsCompleted), CommandParameter = taskItem });
                        break;
                }
            }

            contextMenu.ShowAt(taskItem);
        }

        private void RenameTask(TaskItem taskItem)
        {
            taskItem.IsEditModeEnabled = true;
        }

        private async void DeleteTask(TaskItem taskItem)
        {
            var task = taskItem.TaskDetails;
            var taskListId = taskItem.TaskListId;

            // "New" tasks aren't actually saved to the Graph yet.
            if (!task.IsNew())
            {
                try
                {
                    // Delete task from Graph.
                    var graph = ProviderManager.Instance.GlobalProvider.Graph;
                    await graph.Me.Todo.Lists[taskListId].Tasks[task.Id].Request().DeleteAsync();
                }
                catch
                {
                    // TODO: Handle failure to delete the task.
                    return;
                }
            }

            if (task.Status == Microsoft.Graph.TaskStatus.Completed)
            {
                foreach (var ct in CompletedTasks)
                {
                    if (ct.TaskDetails.Id == task.Id)
                    {
                        CompletedTasks.Remove(ct);
                        break;
                    }
                }
            }
            else
            {
                foreach (var at in AvailableTasks)
                {
                    if (at.TaskDetails.Id == task.Id)
                    {
                        AvailableTasks.Remove(at);
                        break;
                    }
                }
            }
        }

        private void OpenTaskInToDoApp(TaskItem taskItem)
        {
            System.Diagnostics.Debug.WriteLine("Open task in To Do");
        }

        private void LaunchToDoApp()
        {
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

        private void SelectTaskForFocusSession(TaskItem taskItem)
        {
            System.Diagnostics.Debug.WriteLine("Select task for focus session");

            if (taskItem.TaskDetails.IsNew())
            {
                var itemIndex = _availableTasksListView.IndexFromContainer(taskItem);
                _availableTasksListView.SelectRange(new ItemIndexRange(itemIndex, 1));
            }
        }

        private async void MarkTaskAsCompleted(TaskItem taskItem)
        {
            System.Diagnostics.Debug.WriteLine("Mark task as completed");
            var task = taskItem.TaskDetails;
            var previousTaskStatus = task.Status;
            task.Status = Microsoft.Graph.TaskStatus.Completed;

            try
            {
                var graph = ProviderManager.Instance.GlobalProvider.Graph;
                await graph.Me.Todo.Lists[taskItem.TaskListId].Tasks[task.Id].Request().UpdateAsync(task);
            }
            catch
            {
                // Handle failure to mark the task as completed.
                task.Status = previousTaskStatus;
                return;
            }

            foreach (var at in AvailableTasks)
            {
                if (at.TaskDetails.Id == task.Id)
                {
                    AvailableTasks.Remove(at);
                    break;
                }
            }

            CompletedTasks.Add(new TaskItemData(task, TaskListId));
        }

        private async void UnmarkTaskAsCompleted(TaskItem taskItem)
        {
            System.Diagnostics.Debug.WriteLine("Unmark task as completed");
            var task = taskItem.TaskDetails;
            var previousTaskStatus = task.Status;
            task.Status = Microsoft.Graph.TaskStatus.NotStarted;

            try
            {
                var graph = ProviderManager.Instance.GlobalProvider.Graph;
                await graph.Me.Todo.Lists[taskItem.TaskListId].Tasks[task.Id].Request().UpdateAsync(task);
            }
            catch
            {
                // Handle failure to mark the task as completed.
                task.Status = previousTaskStatus;
                return;
            }

            foreach (var ct in CompletedTasks)
            {
                if (ct.TaskDetails.Id == task.Id)
                {
                    CompletedTasks.Remove(ct);
                    break;
                }
            }

            AvailableTasks.Add(new TaskItemData(task, TaskListId));
        }

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
