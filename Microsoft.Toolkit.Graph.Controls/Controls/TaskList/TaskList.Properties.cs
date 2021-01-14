using System.Collections.Generic;
using Microsoft.Graph;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TaskList : BaseGraphControl
    {
        /// <summary>
        /// Gets or sets the TaskDetails property value.
        /// </summary>
        public List<TodoTask> AvailableTasks
        {
            get { return (List<TodoTask>)GetValue(AvailableTasksProperty); }
            set { SetValue(AvailableTasksProperty, value); }
        }

        /// <summary>
        /// Todo task item metadata.
        /// </summary>
        public static readonly DependencyProperty AvailableTasksProperty =
            DependencyProperty.Register(nameof(AvailableTasks), typeof(List<TodoTask>), typeof(TaskList), new PropertyMetadata(new List<TodoTask>()));

        /// <summary>
        /// Gets or sets the TaskDetails property value.
        /// </summary>
        public List<TodoTask> CompletedTasks
        {
            get { return (List<TodoTask>)GetValue(CompletedTasksProperty); }
            set { SetValue(CompletedTasksProperty, value); }
        }

        /// <summary>
        /// Todo task item metadata.
        /// </summary>
        public static readonly DependencyProperty CompletedTasksProperty =
            DependencyProperty.Register(nameof(CompletedTasks), typeof(List<TodoTask>), typeof(TaskList), new PropertyMetadata(new List<TodoTask>()));
    }
}
