using System;
using Microsoft.Graph;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Graph.Controls.Converters
{
    /// <summary>
    /// Converts a TodoTask TaskStatus enum value to True for Completed tasks. Otherwise False.
    /// </summary>
    public class TaskStatusToBoolConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TaskStatus taskStatus)
            {
                return taskStatus == TaskStatus.Completed;
            }

            return null;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
