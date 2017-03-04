using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudDriveShell.Infrastructure.Models
{
    public class LeftMenu
    {
        public LeftMenu(string title, string image, string viewName)
        {
            this.Title = title;
            this.Image = image;
            this.ViewName = viewName;
        }

        public string Title { get; private set; }

        public string Image { get; private set; }

        public string ViewName { get; private set; }
    }
}
