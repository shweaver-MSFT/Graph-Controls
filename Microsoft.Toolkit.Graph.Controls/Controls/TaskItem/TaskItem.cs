using System.Threading.Tasks;

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
        protected override Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task ClearDataAsync()
        {
            TaskDetails = null;
            return Task.CompletedTask;
        }
    }
}
