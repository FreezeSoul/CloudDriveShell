using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CloudDriveShell.Infrastructure.Utils;

namespace CloudDriveShell.Infrastructure.Converters
{
    public class FileSizeHumanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                long size;
                if (long.TryParse(value.ToString(), out size))
                {
                    return FileHelper.ConvertToHumanSize(size);
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
