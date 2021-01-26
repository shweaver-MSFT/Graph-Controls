using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
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
    [TemplatePart(Name = AddButtonPart, Type = typeof(ButtonBase))]
    [TemplatePart(Name = OptionsButtonPart, Type = typeof(ButtonBase))]
    [TemplatePart(Name = AvailableTasksListViewPart, Type = typeof(ListView))]
    [TemplatePart(Name = CompletedTasksListViewPart, Type = typeof(ListView))]
    public partial class TaskList : BaseGraphControl
    {
        private const string AddButtonPart = "PART_AddButton";
        private const string OptionsButtonPart = "PART_OptionsButton";
        private const string AvailableTasksListViewPart = "PART_AvailableTasksListView";
        private const string CompletedTasksListViewPart = "PART_CompletedTasksListView";

        private Button _addButton;
        private Button _optionsButton;
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

            if (_addButton != null)
            {
                _addButton.Click -= AddButton_Click;
            }

            _addButton = GetTemplateChild(AddButtonPart) as Button;

            if (_addButton != null)
            {
                _addButton.Click += AddButton_Click;
            }

            if (_optionsButton != null)
            {
                _optionsButton.Click -= OptionsButton_Click;
            }

            _optionsButton = GetTemplateChild(OptionsButtonPart) as Button;

            if (_optionsButton != null)
            {
                _optionsButton.Click += OptionsButton_Click;
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
            VisualStateManager.GoToState(this, "Loading", false);

            // Fetch the Graph Data
            try
            {
                var graph = ProviderManager.Instance.GlobalProvider.Graph;
                var taskListsPage = await graph.Me.Todo.Lists.Request().GetAsync();
                TaskLists = taskListsPage.CurrentPage;
            }
            catch
            {
            }

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
                System.Diagnostics.Debug.WriteLine(SelectedTaskList.Id);

                var tasks = SelectedTaskList.Tasks;
                if (tasks != null && tasks.Count() > 0)
                {
                    foreach (TodoTask task in tasks)
                    {
                        switch (task.Status)
                        {
                            case Microsoft.Graph.TaskStatus.Completed:
                                CompletedTasks.Add(task);
                                break;
                            default:
                                AvailableTasks.Add(task);
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (_availableTasksListView != null && _availableTasksListView.Items.Count > 0 && _availableTasksListView.Items[0] is TodoTask task && task.CreatedDateTime == null)
                {
                    return;
                }

                AvailableTasks.Insert(0, new TodoTask());
            });
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Options clicked");
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
            if (taskItem.IsEditModeEnabled)
            {
                return;
            }

            var task = taskItem.TaskDetails;

            var contextMenu = new MenuFlyout();
            contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Rename task", Command = new DelegateCommand<TaskItem>(RenameTask), CommandParameter = taskItem });
            contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Delete task", Command = new DelegateCommand<TaskItem>(DeleteTask), CommandParameter = taskItem });
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

            contextMenu.ShowAt(taskItem);
        }

        private void RenameTask(TaskItem taskItem)
        {
            taskItem.IsEditModeEnabled = true;
        }

        private void DeleteTask(TaskItem taskItem)
        {
            var task = taskItem.TaskDetails;

            // TODO: Delete task from Graph.

            if (task.Status == Microsoft.Graph.TaskStatus.Completed)
            {
                CompletedTasks.Remove(task);
            }
            else
            {
                AvailableTasks.Remove(task);
            }
        }

        private void OpenTaskInToDoApp(TaskItem taskItem)
        {
            System.Diagnostics.Debug.WriteLine("Open task in To Do");
        }

        private void SelectTaskForFocusSession(TaskItem taskItem)
        {
            System.Diagnostics.Debug.WriteLine("Select task for focus session");
        }

        private void MarkTaskAsCompleted(TaskItem taskItem)
        {
            System.Diagnostics.Debug.WriteLine("Mark task as completed");
            var task = taskItem.TaskDetails;

            task.Status = Microsoft.Graph.TaskStatus.Completed;

            AvailableTasks.Remove(task);
            CompletedTasks.Add(task);
        }

        private void UnmarkTaskAsCompleted(TaskItem taskItem)
        {
            System.Diagnostics.Debug.WriteLine("Unmark task as completed");
            var task = taskItem.TaskDetails;

            task.Status = Microsoft.Graph.TaskStatus.NotStarted;

            CompletedTasks.Remove(task);
            AvailableTasks.Add(task);
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
    }
}
