using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// foo
    /// </summary>
    [TemplatePart(Name = AddButtonPart, Type = typeof(ButtonBase))]
    [TemplatePart(Name = OptionsButtonPart, Type = typeof(ButtonBase))]
    public partial class TaskList : BaseGraphControl
    {
        private const string AddButtonPart = "PART_AddButton";
        private const string OptionsButtonPart = "PART_OptionsButton";

        private Button _addButton;
        private Button _optionsButton;

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
                CompletedTasks.Add(new TodoTask() { Title = "This is a test" });
                System.Diagnostics.Debug.WriteLine("Add clicked: " + CompletedTasks.Count());
            });
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Options clicked");
        }
    }
}
