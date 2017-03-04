using System.Globalization;
using System.Windows.Data;
using CloudDriveShell.Infrastructure.Models;

namespace CloudDriveShell.Infrastructure.Converters
{
    public class NavigationIconSourceConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            var obj = value as NavigateMenu;
            if (obj != null)
                return System.String.Format(CultureInfo.CurrentUICulture, "/{0};component/{1}", obj.AssemblyName, obj.MenuIcon);
            return null;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
