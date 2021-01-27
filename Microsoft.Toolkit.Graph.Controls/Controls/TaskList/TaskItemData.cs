using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// A construct for holding necessary data to update the remote Graph task.
    /// </summary>
    public struct TaskItemData
    {
        /// <summary>
        /// Gets or sets the TodoTask details.
        /// </summary>
        public TodoTask TaskDetails { get; set; }

        /// <summary>
        /// Gets or sets the associated task list id.
        /// </summary>
        public string TaskListId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskItemData"/> struct.
        /// </summary>
        /// <param name="taskDetails"></param>
        /// <param name="taskListId"></param>
        public TaskItemData(TodoTask taskDetails, string taskListId)
        {
            TaskDetails = taskDetails;
            TaskListId = taskListId;
        }
    }
}
