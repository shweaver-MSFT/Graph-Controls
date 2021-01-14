using Microsoft.Graph;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// foo
    /// </summary>
    public partial class TodoItem
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
        /// TodoTask item metadata.
        /// </summary>
        public static readonly DependencyProperty TaskDetailsProperty =
            DependencyProperty.Register(nameof(TaskDetails), typeof(TodoTask), typeof(TodoItem), new PropertyMetadata(null));
    }
}
