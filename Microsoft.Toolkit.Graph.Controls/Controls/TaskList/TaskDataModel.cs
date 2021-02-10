using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// A wrapper struct for the TodoTask data.
    ///
    /// The TodoTask object doesn't include the id of the parent TaskList.
    /// Yet, we need it for making CRUD calls to the Graph.
    /// </summary>
    public struct TaskDataModel
    {
        /// <summary>
        /// Gets or sets the id of the associated TaskList id, required for Graph calls.
        /// </summary>
        public string TaskListId { get; set; }

        /// <summary>
        /// Gets or sets the TodoTask object.
        /// </summary>
        public TodoTask Task { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDataModel"/> struct.
        /// </summary>
        /// <param name="taskListId"></param>
        /// <param name="task"></param>
        public TaskDataModel(string taskListId, TodoTask task)
        {
            TaskListId = taskListId;
            Task = task;
        }
    }
}
