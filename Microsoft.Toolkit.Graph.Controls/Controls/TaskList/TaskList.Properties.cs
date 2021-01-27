using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TaskList : BaseGraphControl
    {
        public bool IsContentCollapsed
        {
            get { return (bool)GetValue(IsContentCollapsedProperty); }
            set { SetValue(IsContentCollapsedProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty IsContentCollapsedProperty =
            DependencyProperty.Register(nameof(IsContentCollapsed), typeof(bool), typeof(TaskList), new PropertyMetadata(false, OnIsContentCollapsedChanged));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        public static void OnIsContentCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TaskList taskList)
            {
                if (taskList.IsContentCollapsed)
                {
                    taskList.GoToVisualState("Collapsed", true);
                }
                else
                {
                    switch (ProviderManager.Instance.GlobalProvider.State)
                    {
                        case ProviderState.SignedOut:
                            taskList.GoToVisualState(CommonStates.SignedOut, true);
                            break;
                        case ProviderState.Loading:
                            taskList.GoToVisualState(CommonStates.Loading, true);
                            break;
                        case ProviderState.SignedIn:
                            taskList.GoToVisualState(CommonStates.SignedIn, true);
                            break;
                        default:
                            taskList.GoToVisualState(CommonStates.Error, true);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the TaskListId property value. Used to set the default TaskList.
        /// </summary>
        public string TaskListId
        {
            get { return (string)GetValue(TaskListIdProperty); }
            set { SetValue(TaskListIdProperty, value); }
        }

        /// <summary>
        /// Todo task list default id.
        /// </summary>
        public static readonly DependencyProperty TaskListIdProperty =
            DependencyProperty.Register(nameof(TaskListId), typeof(string), typeof(TaskList), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the TaskLists property value.
        /// </summary>
        public IList<TodoTaskList> TaskLists
        {
            get { return (IList<TodoTaskList>)GetValue(TaskListsProperty); }
            set { SetValue(TaskListsProperty, value); }
        }

        /// <summary>
        /// Todo task lists.
        /// </summary>
        public static readonly DependencyProperty TaskListsProperty =
            DependencyProperty.Register(nameof(TaskLists), typeof(IList<TodoTaskList>), typeof(TaskList), new PropertyMetadata(new ObservableCollection<TodoTaskList>()));

        /// <summary>
        /// Gets or sets the SelectedTaskIndex property value.
        /// </summary>
        public int SelectedTaskListIndex
        {
            get { return (int)GetValue(SelectedTaskListIndexProperty); }
            set { SetValue(SelectedTaskListIndexProperty, value); }
        }

        /// <summary>
        /// Todo task lists.
        /// </summary>
        public static readonly DependencyProperty SelectedTaskListIndexProperty =
            DependencyProperty.Register(nameof(SelectedTaskListIndex), typeof(int), typeof(TaskList), new PropertyMetadata(0, OnSelectedTaskListIndexChanged));

        /// <summary>
        /// foo
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnSelectedTaskListIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TaskList taskList)
            {
                taskList.SelectedTaskList = (TodoTaskList)e.NewValue;
                System.Diagnostics.Debug.WriteLine(taskList.SelectedTaskList.Id);
            }
        }

        /// <summary>
        /// Gets or sets the SelectedTaskList property value.
        /// </summary>
        public TodoTaskList SelectedTaskList { get; protected set; }

        /// <summary>
        /// Gets or sets the TaskDetails property value.
        /// </summary>
        public IList<TaskItemData> AvailableTasks
        {
            get { return (IList<TaskItemData>)GetValue(AvailableTasksProperty); }
            set { SetValue(AvailableTasksProperty, value); }
        }

        /// <summary>
        /// Todo task item metadata.
        /// </summary>
        public static readonly DependencyProperty AvailableTasksProperty =
            DependencyProperty.Register(nameof(AvailableTasks), typeof(IList<TaskItemData>), typeof(TaskList), new PropertyMetadata(new ObservableCollection<TaskItemData>()));

        /// <summary>
        /// Gets or sets the TaskDetails property value.
        /// </summary>
        public IList<TaskItemData> CompletedTasks
        {
            get { return (IList<TaskItemData>)GetValue(CompletedTasksProperty); }
            set { SetValue(CompletedTasksProperty, value); }
        }

        /// <summary>
        /// Todo task item metadata.
        /// </summary>
        public static readonly DependencyProperty CompletedTasksProperty =
            DependencyProperty.Register(nameof(CompletedTasks), typeof(IList<TaskItemData>), typeof(TaskList), new PropertyMetadata(new ObservableCollection<TaskItemData>()));
    }
}
