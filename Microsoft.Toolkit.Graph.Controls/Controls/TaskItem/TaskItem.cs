using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Graph.Controls.Extensions;
using Microsoft.Toolkit.Graph.Providers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// A visualization of a single Todo task item.
    /// </summary>
    [TemplatePart(Name = TaskTitleInputTextBoxPart, Type = typeof(TextBox))]
    public partial class TaskItem : BaseGraphControl
    {
        private const string TaskTitleInputTextBoxPart = "PART_TaskTitleInputTextBox";

        private TextBox _taskTitleInputTextBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskItem"/> class.
        /// </summary>
        public TaskItem()
        {
            this.DefaultStyleKey = typeof(TaskItem);
        }

        /// <summary>
        ///  <inheritdoc />
        /// </summary>
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
        }

        private async void TaskTitleInputTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                await TrySaveAsync();
            }
        }

        /// <inheritdoc/>
        protected override Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task ClearDataAsync()
        {
            TaskDetails = null;
            IsEditModeEnabled = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Attempt to save any edits to the task.
        /// </summary>
        /// <returns>Success or failure boolean value.</returns>
        protected async Task<bool> TrySaveAsync()
        {
            try
            {
                var task = TaskDetails;
                var taskListId = TaskListId;

                var inputText = _taskTitleInputTextBox.Text;
                if (string.IsNullOrWhiteSpace(inputText))
                {
                    return false;
                }

                TaskDetails.Title = inputText;

                var graph = ProviderManager.Instance.GlobalProvider.Graph;
                var updatedTask = task.IsNew()
                    ? await graph.Me.Todo.Lists[taskListId].Tasks.Request().AddAsync(task)
                    : await graph.Me.Todo.Lists[taskListId].Tasks[task.Id].Request().UpdateAsync(task);

                TaskDetails = updatedTask;

                IsEditModeEnabled = false;
                this.Focus(FocusState.Programmatic);
                return true;
            }
            catch (Exception e)
            {
                // TODO: Handle failure to save modified task details
                return false;
            }
        }
    }
}
