using Microsoft.Graph;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// foo
    /// </summary>
    public partial class TaskItem
    {
        /// <summary>
        /// Gets or sets the TaskDetails property value.
        /// </summary>
        public TodoTask TaskDetails
        {
            get { return (TodoTask)GetValue(TaskDetailsProperty); }
            set { SetValue(TaskDetailsProperty, value); }
        }

        /// <summary>
        /// Todo task item metadata.
        /// </summary>
        public static readonly DependencyProperty TaskDetailsProperty =
            DependencyProperty.Register(nameof(TaskDetails), typeof(TodoTask), typeof(TaskItem), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value indicating whether the control is in an editable state.
        /// </summary>
        public bool IsEditModeEnabled
        {
            get { return (bool)GetValue(IsEditModeEnabledProperty); }
            set { SetValue(IsEditModeEnabledProperty, value); }
        }

        /// <summary>
        /// Edit mode enablement property.
        /// </summary>
        public static readonly DependencyProperty IsEditModeEnabledProperty =
            DependencyProperty.Register(nameof(IsEditModeEnabled), typeof(bool), typeof(TaskItem), new PropertyMetadata(false));
    }
}
