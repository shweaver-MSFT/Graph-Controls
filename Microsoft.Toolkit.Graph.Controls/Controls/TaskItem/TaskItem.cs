using System;
using System.Threading.Tasks;
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
    [TemplatePart(Name = TaskStatusCheckBoxPart, Type = typeof(CheckBox))]
    public partial class TaskItem : BaseGraphControl
    {
        private const string TaskTitleInputTextBoxPart = "PART_TaskTitleInputTextBox";
        private const string TaskStatusCheckBoxPart = "PART_TaskStatusCheckBox";

        private TextBox _taskTitleInputTextBox;
        private CheckBox _taskStatusCheckBox;

        /// <summary>
        /// 
        /// </summary>
        public event RoutedEventHandler Checked;

        /// <summary>
        /// 
        /// </summary>
        public event RoutedEventHandler Indeterminate;

        /// <summary>
        /// 
        /// </summary>
        public event RoutedEventHandler Unchecked;

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

            if (_taskStatusCheckBox != null)
            {
                _taskStatusCheckBox.Checked -= TaskStatusCheckBox_Checked;
                _taskStatusCheckBox.Indeterminate -= TaskStatusCheckBox_Indeterminate;
                _taskStatusCheckBox.Unchecked -= TaskStatusCheckBox_Unchecked;
            }

            _taskStatusCheckBox = GetTemplateChild(TaskStatusCheckBoxPart) as CheckBox;

            if (_taskStatusCheckBox != null)
            {
                _taskStatusCheckBox.Checked += TaskStatusCheckBox_Checked;
                _taskStatusCheckBox.Indeterminate += TaskStatusCheckBox_Indeterminate;
                _taskStatusCheckBox.Unchecked += TaskStatusCheckBox_Unchecked;
            }
        }

        private void TaskStatusCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // TODO: Save 
            Checked?.Invoke(this, e);
        }

        private void TaskStatusCheckBox_Indeterminate(object sender, RoutedEventArgs e)
        {
            Indeterminate?.Invoke(this, e);
        }

        private void TaskStatusCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

            Unchecked?.Invoke(this, e);
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
        public async Task<bool> TrySaveAsync()
        {
            var inputText = _taskTitleInputTextBox.Text;
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return false;
            }

            TaskDetails.Title = inputText.Trim();
            var taskData = new TaskItemData(TaskDetails, TaskListId);

            try
            {
                TaskDetails = taskData.IsNew
                    ? await TaskItemDataSource.AddTaskAsync(taskData.TaskListId, taskData.TaskDetails)
                    : await TaskItemDataSource.UpdateTaskAsync(taskData.TaskListId, taskData.TaskDetails);
            }
            catch
            {
                // TODO: Handle failure to save modified task details
                return false;
            }

            TaskTitle = TaskDetails.Title;
            IsEditModeEnabled = false;
            this.Focus(FocusState.Programmatic);
            return true;
        }
    }
}
