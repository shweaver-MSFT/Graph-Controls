using System;
using Microsoft.Graph;
using Windows.UI.Text;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Graph.Controls.Converters
{
    /// <summary>
    /// Converts a TaskStatus value to the appropriate TextDecorations value.
    /// </summary>
    public class TaskStatusToTextDecorationsConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TaskStatus taskStatus)
            {
                switch (taskStatus)
                {
                    case TaskStatus.Completed:
                        return TextDecorations.Strikethrough;
                    default:
                        return TextDecorations.None;
                }
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
