using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;

namespace Microsoft.Toolkit.Graph.Controls.Controls.TaskList
{
    /// <summary>
    /// The data source handles all service interaction for CRUD operations on Graph Todo Tasks.
    /// </summary>
    public class TaskItemDataSource
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

            ITodoListsCollectionPage taskListsPage = await Graph.Me.Todo.Lists.Request().GetAsync();
            return taskListsPage.CurrentPage;
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
