using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Controls.Extensions;
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
            if (item is TodoTask task && task.IsNew())
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
