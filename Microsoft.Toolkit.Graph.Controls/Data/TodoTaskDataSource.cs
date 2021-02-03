// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;

namespace Microsoft.Toolkit.Graph.Controls.Data
{
    /// <summary>
    /// The data source handles all service interaction for CRUD operations on Graph Todo Tasks.
    /// </summary>
    public class TodoTaskDataSource
    {
        private static readonly bool FakeIt = false;

        private static GraphServiceClient Graph => ProviderManager.Instance.GlobalProvider.Graph;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<IList<TodoTaskList>> GetMyTaskListsAsync()
        {
            if (FakeIt)
            {
                return new List<TodoTaskList>()
                {
                    new TodoTaskList()
                    {
                        DisplayName = "My Day",
                        Id = "MyDay",
                    },
                };
            }

            ITodoListsCollectionPage taskListsCollection = await Graph.Me.Todo.Lists.Request().GetAsync();
            return taskListsCollection.CurrentPage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskListId"></param>
        /// <returns></returns>
        public static async Task<IList<TodoTask>> GetTasksAsync(string taskListId)
        {
            if (FakeIt)
            {
                return new List<TodoTask>()
                {
                    new TodoTask() { Title = "Plant a house." },
                    new TodoTask() { Title = "Build a tree." },
                };
            }

            ITodoTaskListTasksCollectionPage tasksCollection = await Graph.Me.Todo.Lists[taskListId].Tasks.Request().GetAsync();
            return tasksCollection.CurrentPage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskListId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public static async Task<TodoTask> GetTaskAsync(string taskListId, string taskId)
        {
            if (FakeIt)
            {
                return new TodoTask() { Title = "Plant a house." };
            }

            TodoTask task = await Graph.Me.Todo.Lists[taskListId].Tasks[taskId].Request().GetAsync();
            return task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="taskListId"></param>
        /// <returns></returns>
        public static async Task DeleteTaskAsync(string taskListId, string taskId)
        {
            if (FakeIt)
            {
                return;
            }

            await Graph.Me.Todo.Lists[taskListId].Tasks[taskId].Request().DeleteAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskListId"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<TodoTask> UpdateTaskAsync(string taskListId, TodoTask task)
        {
            if (FakeIt)
            {
                return task;
            }

            return await Graph.Me.Todo.Lists[taskListId].Tasks[task.Id].Request().UpdateAsync(task);
        }

        /// <summary>
        /// Create a new task.
        /// </summary>
        /// <param name="taskListId"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<TodoTask> AddTaskAsync(string taskListId, TodoTask task)
        {
            if (FakeIt)
            {
                task.Id = DateTimeOffset.Now.ToString();
                task.CreatedDateTime = DateTimeOffset.Now;
                return task;
            }

            return await Graph.Me.Todo.Lists[taskListId].Tasks.Request().AddAsync(task);
        }
    }
}
