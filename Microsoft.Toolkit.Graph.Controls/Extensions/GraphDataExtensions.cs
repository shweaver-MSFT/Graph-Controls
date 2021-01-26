using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.Controls.Extensions
{
    /// <summary>
    /// Extension methods for Graph data constructs.
    /// </summary>
    public static class GraphDataExtensions
    {
        /// <summary>
        /// Determines whether a TodoTask is "New", meaning not yet saved to the Graph.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static bool IsNew(this TodoTask task)
        {
            return task != null && task.CreatedDateTime == null;
        }
    }
}
