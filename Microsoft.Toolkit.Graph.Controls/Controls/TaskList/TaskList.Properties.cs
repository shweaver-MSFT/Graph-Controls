// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// Gets or sets a value indicating whether the main content panel is visible or not.
        /// </summary>
        public bool IsContentCollapsed
        {
            get { return (bool)GetValue(IsContentCollapsedProperty); }
            set { SetValue(IsContentCollapsedProperty, value); }
        }

        /// <summary>
        /// IsContentCollapsed dependency property.
        /// </summary>
        public static readonly DependencyProperty IsContentCollapsedProperty =
            DependencyProperty.Register(nameof(IsContentCollapsed), typeof(bool), typeof(TaskList), new PropertyMetadata(false, OnIsContentCollapsedChanged));

        /// <summary>
        /// Handle the toggle of the collapsed state.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        public static void OnIsContentCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TaskList taskList)
            {
                taskList.UpdateVisualState();
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
        /// Todo task lists dependency property.
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
        /// Selected Todo task list index dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedTaskListIndexProperty =
            DependencyProperty.Register(nameof(SelectedTaskListIndex), typeof(int), typeof(TaskList), new PropertyMetadata(0, OnSelectedTaskListIndexChanged));

        /// <summary>
        /// foo
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static async void OnSelectedTaskListIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            await d.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (d is TaskList taskList)
                {
                    taskList.LoadTasks();
                }
            });
        }

        /// <summary>
        /// Gets or sets the SelectedTaskList property value.
        /// </summary>
        public TodoTaskList SelectedTaskList { get; protected set; }

        /// <summary>
        /// Gets or sets the TaskDetails property value.
        /// </summary>
        public IList<TaskDataModel> AvailableTasks
        {
            get { return (IList<TaskDataModel>)GetValue(AvailableTasksProperty); }
            set { SetValue(AvailableTasksProperty, value); }
        }

        /// <summary>
        /// Todo task item metadata.
        /// </summary>
        public static readonly DependencyProperty AvailableTasksProperty =
            DependencyProperty.Register(nameof(AvailableTasks), typeof(IList<TaskDataModel>), typeof(TaskList), new PropertyMetadata(new ObservableCollection<TaskDataModel>()));

        /// <summary>
        /// Gets or sets the TaskDetails property value.
        /// </summary>
        public IList<TaskDataModel> CompletedTasks
        {
            get { return (IList<TaskDataModel>)GetValue(CompletedTasksProperty); }
            set { SetValue(CompletedTasksProperty, value); }
        }

        /// <summary>
        /// Todo task item metadata.
        /// </summary>
        public static readonly DependencyProperty CompletedTasksProperty =
            DependencyProperty.Register(nameof(CompletedTasks), typeof(IList<TaskDataModel>), typeof(TaskList), new PropertyMetadata(new ObservableCollection<TaskDataModel>()));
    }
}
