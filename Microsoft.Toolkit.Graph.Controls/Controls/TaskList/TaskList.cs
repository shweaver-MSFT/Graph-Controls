using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// foo
    /// </summary>
    [TemplatePart(Name = AddButtonPart, Type = typeof(ButtonBase))]
    [TemplatePart(Name = OptionsButtonPart, Type = typeof(ButtonBase))]
    [TemplatePart(Name = AvailableTasksListViewPart, Type = typeof(ListView))]
    public partial class TaskList : BaseGraphControl
    {
        private const string AddButtonPart = "PART_AddButton";
        private const string OptionsButtonPart = "PART_OptionsButton";
        private const string AvailableTasksListViewPart = "PART_AvailableTasksListView";

        private Button _addButton;
        private Button _optionsButton;
        private ListView _availableTasksListView;

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
        }

        /// <inheritdoc/>
        protected override async Task LoadDataAsync()
        {
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
        protected override Task ClearDataAsync()
        {
            TaskLists.Clear();
            CompletedTasks.Clear();
            AvailableTasks.Clear();
            SelectedTaskListIndex = 0;

            return Task.CompletedTask;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                AvailableTasks.Add(new TodoTask() { Title = "This is a test" });
                System.Diagnostics.Debug.WriteLine("Add clicked: " + AvailableTasks.Count());
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
            System.Diagnostics.Debug.WriteLine("Context requested");
            var element = e.OriginalSource as FrameworkElement;
            var task = element.DataContext as TodoTask;
            ShowTaskItemContextMenu(task, element);
        }

        private void ShowTaskItemContextMenu(TodoTask task, FrameworkElement targetElement)
        {
            var contextMenu = new MenuFlyout();
            contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Rename task", Command = new DelegateCommand<TodoTask>(RenameTask), CommandParameter = task });
            contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Delete task", Command = new DelegateCommand<TodoTask>(DeleteTask), CommandParameter = task });
            contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Open in To Do", Command = new DelegateCommand<TodoTask>(OpenTaskInToDoApp), CommandParameter = task });

            switch (task.Status)
            {
                case Microsoft.Graph.TaskStatus.Completed:
                    break;

                default:
                    contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Select for focus session", Command = new DelegateCommand<TodoTask>(SelectTaskForFocusSession), CommandParameter = task });
                    contextMenu.Items.Add(new MenuFlyoutItem() { Text = "Mark as completed", Command = new DelegateCommand<TodoTask>(SelectTaskForFocusSession), CommandParameter = task });
                    break;
            }

            contextMenu.ShowAt(targetElement);
        }

        private void RenameTask(TodoTask task)
        {
            System.Diagnostics.Debug.WriteLine("Rename task");
        }

        private void DeleteTask(TodoTask task)
        {
            System.Diagnostics.Debug.WriteLine("Delete task");
        }

        private void OpenTaskInToDoApp(TodoTask task)
        {
            System.Diagnostics.Debug.WriteLine("Open task in To Do");
        }

        private void SelectTaskForFocusSession(TodoTask task)
        {
            System.Diagnostics.Debug.WriteLine("Open task in To Do");
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
