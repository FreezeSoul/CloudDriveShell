using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Data;

namespace CloudDriveShell.Infrastructure.Converters
{
    public class NavigatePathNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return string.Empty;
            var list = value.ToString().TrimEnd('/').Split('/');
            if (list.Length >= 1)
                return HttpUtility.UrlDecode(list[list.Length - 1]);
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NavigateShortPathNameConverter : IValueConverter
    {
        private const int length = 10;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return string.Empty;
            var list = value.ToString().TrimEnd('/').Split('/');
            if (list.Length >= 1)
            {
                var name = HttpUtility.UrlDecode(list[list.Length - 1]);
                if (name != null && name.Length > length)
                {
                    name = string.Format("{0}...", name.Substring(0, length));
                }
                return name;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
