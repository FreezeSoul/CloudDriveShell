using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudDriveShell.Infrastructure.Models
{
    public class AccountInfo : INotifyPropertyChanged
    {

        private string _accountName;

        public string AccountName
        {
            get { return _accountName; }
            set { _accountName = value; this.NotifyPropertyChanged("AccountName"); }
        }

        private bool _canEdit = true;

        public bool CanEdit
        {
            get { return _canEdit; }
            set { _canEdit = value; this.NotifyPropertyChanged("CanEdit"); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    public static class CurrentAccountInfo 
    {
        public static readonly AccountInfo Instance  = new AccountInfo();
    }
}
