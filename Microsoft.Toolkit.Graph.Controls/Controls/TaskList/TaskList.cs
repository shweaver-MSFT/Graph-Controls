using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TaskList : BaseGraphControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskList"/> class.
        /// </summary>
        public TaskList()
        {
            this.DefaultStyleKey = typeof(TaskList);
        }

        /// <inheritdoc/>
        protected override void LoadData()
        {
            CompletedTasks.Add(new TodoTask()
            {
                Title = "Walk the dog",
                Status = TaskStatus.Completed,
            });
            AvailableTasks.Add(new TodoTask()
            {
                Title = "Do the dishes",
                Status = TaskStatus.InProgress,
            });
        }

        /// <inheritdoc/>
        protected override void ClearData()
        {
            CompletedTasks.Clear();
            AvailableTasks.Clear();
        }
    }
}
