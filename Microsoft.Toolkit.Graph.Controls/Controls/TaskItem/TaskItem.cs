using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
                _taskTitleInputTextBox.LostFocus -= TaskTitleInputTextBox_LostFocus;
            }

            _taskTitleInputTextBox = GetTemplateChild(TaskTitleInputTextBoxPart) as TextBox;

            if (_taskTitleInputTextBox != null)
            {
                _taskTitleInputTextBox.LostFocus += TaskTitleInputTextBox_LostFocus;
            }
        }

        private void TaskTitleInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            IsEditModeEnabled = false;
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
    }
}
