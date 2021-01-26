using Microsoft.Graph;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// Selects the appropriate template based upon the item metadata.
    /// New tasks should appear ready for input.
    /// </summary>
    public class TaskItemTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the default template for TaskItems.
        /// </summary>
        public DataTemplate Normal { get; set; }

        /// <summary>
        /// Gets or sets the template for a newly created task.
        /// </summary>
        public DataTemplate NewTask { get; set; }

        /// <inheritdoc />
        protected override DataTemplate SelectTemplateCore(object item)
        {
            var task = item as TodoTask;
            if (task != null && task.CreatedDateTime == null)
            {
                return NewTask;
            }
            else
            {
                return Normal;
            }
        }
    }
}
