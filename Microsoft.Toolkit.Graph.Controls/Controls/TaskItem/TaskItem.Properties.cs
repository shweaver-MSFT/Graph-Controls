using Microsoft.Graph;
using System;
using System.ComponentModel;
using Windows.UI.Core;
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
            DependencyProperty.Register(nameof(TaskDetails), typeof(TodoTask), typeof(TaskItem), new PropertyMetadata(null, OnTaskDetailsChanged));

        private static async void OnTaskDetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            await d.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (d is TaskItem taskItem)
                {
                    taskItem.TaskTitle = taskItem.TaskDetails.Title;
                }
            });
        }

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
            DependencyProperty.Register(nameof(IsEditModeEnabled), typeof(bool), typeof(TaskItem), new PropertyMetadata(false, OnEditModeChanged));

        private static async void OnEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            await d.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (d is TaskItem taskItem)
                {
                    if (taskItem.IsEditModeEnabled)
                    {
                        taskItem._taskTitleInputTextBox.Text = taskItem.TaskTitle;
                        taskItem._taskTitleInputTextBox.Focus(FocusState.Programmatic);
                    }
                    else
                    {
                        // Save the updates
                        taskItem.TaskTitle = taskItem._taskTitleInputTextBox.Text;
                        taskItem._taskTitleInputTextBox.Text = string.Empty;
                    }
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public string TaskTitle
        {
            get { return (string)GetValue(TaskTitleProperty); }
            set { SetValue(TaskTitleProperty, value); }
        }

        /// <summary>
        /// Task title input text property.
        /// </summary>
        public static readonly DependencyProperty TaskTitleProperty =
            DependencyProperty.Register(nameof(TaskTitle), typeof(string), typeof(TaskItem), new PropertyMetadata(string.Empty, OnTaskTitleChanged));

        private static async void OnTaskTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            await d.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (d is TaskItem taskItem)
                {
                    taskItem.TaskDetails.Title = taskItem.TaskTitle;
                }
            });
        }
    }
}
