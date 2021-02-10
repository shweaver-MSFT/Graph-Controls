using System.Collections.Generic;
using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.Controls.Extensions
{
    /// <summary>
    /// Extension methods for the TodoTask Graph object type.
    /// </summary>
    public static class TodoTaskExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static bool IsCompleted(this TodoTask task)
        {
            return task.Status == TaskStatus.Completed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static bool IsNew(this TodoTask task)
        {
            return task.CreatedDateTime == null;
        }

        //
        public static void RemoveById(this IList<TodoTask> taskList, string taskId)
        {
            foreach (var task in taskList)
            {
                if (task.Id == taskId)
                {
                    taskList.Remove(task);
                    break;
                }
            }
        }
    }
}
