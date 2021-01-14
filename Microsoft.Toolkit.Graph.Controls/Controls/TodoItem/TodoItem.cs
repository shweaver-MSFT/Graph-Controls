using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// A visualization of a single Todo task item.
    /// </summary>
    public partial class TodoItem : BaseGraphControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TodoItem"/> class.
        /// </summary>
        public TodoItem()
        {
            this.DefaultStyleKey = typeof(TodoItem);
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
