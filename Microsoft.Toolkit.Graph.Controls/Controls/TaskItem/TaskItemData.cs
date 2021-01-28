using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Controls.Controls.TaskList;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// A construct for holding necessary data to update the remote Graph task.
    /// </summary>
    public class TaskItemData : INotifyPropertyChanged
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        private TodoTask _todoTask;

        /// <summary>
        /// Gets or sets the TodoTask details.
        /// </summary>
        public TodoTask TaskDetails
        {
            get => _todoTask;
            set => Set(ref _todoTask, value);
        }

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

        /// <summary>
        /// Gets the Id of the underlying TodoTask.
        /// </summary>
        public string TaskId => TaskDetails?.Id;

        /// <summary>
        /// Gets a value indicating whether the TodoTask is completed.
        /// </summary>
        public bool IsCompleted => TaskDetails?.Status == Microsoft.Graph.TaskStatus.Completed;

        /// <summary>
        /// Gets a value indicating whether the TodoTask is new, not yet saved to the Graph.
        /// </summary>
        public bool IsNew => TaskDetails == null || TaskDetails.CreatedDateTime == null;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> MarkAsCompletedAsync()
        {
            Microsoft.Graph.TaskStatus? previousStatus = TaskDetails.Status;

            try
            {
                TaskDetails.Status = Microsoft.Graph.TaskStatus.Completed;
                await TaskItemDataSource.UpdateTaskAsync(TaskListId, TaskDetails);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TaskDetails)));
                return true;
            }
            catch
            {
                // TODO: Handle failure to mark task as completed
                TaskDetails.Status = previousStatus;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UnmarkAsCompletedAsync()
        {
            Microsoft.Graph.TaskStatus? previousStatus = TaskDetails.Status;

            try
            {
                TaskDetails.Status = Microsoft.Graph.TaskStatus.NotStarted;
                await TaskItemDataSource.UpdateTaskAsync(TaskListId, TaskDetails);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TaskDetails)));
                return true;
            }
            catch
            {
                // TODO: Handle failure to unmark task as completed
                TaskDetails.Status = previousStatus;
                return false;
            }
        }

        private void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;

                if (propertyName != null)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
