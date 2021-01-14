using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// A visualization of a single Todo task item.
    /// </summary>
    public partial class TaskItem : BaseGraphControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskItem"/> class.
        /// </summary>
        public TaskItem()
        {
            this.DefaultStyleKey = typeof(TaskItem);
        }

        /// <inheritdoc/>
        protected override void LoadData()
        {
            TaskDetails = new TodoTask()
            {
                Title = "Walk the dog",
                Status = TaskStatus.Completed,
            };
        }

        /// <inheritdoc/>
        protected override void ClearData()
        {
            TaskDetails = null;
        }
    }
}
