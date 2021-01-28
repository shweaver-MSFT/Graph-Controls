using System.Collections.Generic;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public static class TaskItemDataExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskList"></param>
        /// <param name="taskId"></param>
        public static void RemoveById(this IList<TaskItemData> taskList, string taskId)
        {
            foreach (var task in taskList)
            {
                if (task.TaskId == taskId)
                {
                    taskList.Remove(task);
                    break;
                }
            }
        }
    }
}
