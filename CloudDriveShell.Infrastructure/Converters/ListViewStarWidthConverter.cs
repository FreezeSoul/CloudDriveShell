using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace CloudDriveShell.Infrastructure.Converters
{
    public class ListViewStarWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListView listview = value as ListView;
            if (listview == null) return 0;
            double width = listview.ActualWidth;
            GridView gv = listview.View as GridView;
            if (gv == null) return 0;
            foreach (GridViewColumn t in gv.Columns)
            {
                if (!Double.IsNaN(t.Width))
                    width -= t.Width;
            }
            return width - 50;// this is to take care of margin/padding
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
