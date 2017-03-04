using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure.Utils;

namespace CloudDriveShell.Infrastructure.Models
{
    public class ResourceItem : INotifyPropertyChanged
    {
        private string _itemName;

        public string ItemName
        {
            get { return _itemName; }
            set { _itemName = FileHelper.RemovePathUnSupportChart(value); this.NotifyPropertyChanged("ItemName"); }
        }


        private long? _itemSize;

        public long? ItemSize
        {
            get { return _itemSize; }
            set { _itemSize = value; this.NotifyPropertyChanged("ItemSize"); }
        }

        private string _itemType;

        public string ItemType
        {
            get { return _itemType; }
            set { _itemType = value; this.NotifyPropertyChanged("ItemType"); }
        }
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; this.NotifyPropertyChanged("IsSelected"); }
        }

        private DateTime? _createTime;

        public DateTime? CreateTime
        {
            get { return _createTime; }
            set { _createTime = value; this.NotifyPropertyChanged("CreateTime"); }
        }

        private DateTime? _modifyTime;

        public DateTime? ModifyTime
        {
            get { return _modifyTime; }
            set { _modifyTime = value; this.NotifyPropertyChanged("ModifyTime"); }
        }

        private bool _isFolder;

        public bool IsFolder
        {
            get { return _isFolder; }
            set { _isFolder = value; this.NotifyPropertyChanged("IsFolder"); }
        }

        private string _itemHref;

        public string ItemHref
        {
            get { return _itemHref; }
            set { _itemHref = value; this.NotifyPropertyChanged("ItemHref"); }
        }


        public string ParentHref
        {
            get
            {
                var pathStr = this.ItemHref.TrimEnd('/');
                var lastIndex = pathStr.LastIndexOf('/');
                return string.Format("{0}/", pathStr.Substring(0, lastIndex));
            }
        }

        private bool _isEdit;

        /// <summary>
        /// For UI Editor
        /// </summary>
        public bool IsEdit
        {
            get { return _isEdit; }
            set { _isEdit = value; this.NotifyPropertyChanged("IsEdit"); }
        }

        private bool _isCopyOrCut;

        /// <summary>
        /// For UI Editor
        /// </summary>
        public bool IsCopyOrCut
        {
            get { return _isCopyOrCut; }
            set { _isCopyOrCut = value; this.NotifyPropertyChanged("IsCopyOrCut"); }
        }


        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("资源名称：{0}{1}", this.ItemName, Environment.NewLine);
            stringBuilder.AppendFormat("修改时间：{0}{1}", this.ModifyTime == null ? string.Empty : this.ModifyTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), Environment.NewLine);
            stringBuilder.AppendFormat("资源类型：{0}{1}", this.IsFolder ? "文件夹" : "文件", Environment.NewLine);
            if (!this.IsFolder)
                stringBuilder.AppendFormat("资源大小：{0}{1}", FileHelper.ConvertToHumanSize(this.ItemSize ?? 0), Environment.NewLine);
            stringBuilder.AppendFormat("资源路径：{0}{1}", WebUtility.UrlDecode(this.ItemHref).Replace(WebDavConstant.RootPath, string.Empty).TrimEnd('/'), Environment.NewLine);
            stringBuilder.AppendFormat("操作提示：双击打开文件{0},按住Ctrl多选", this.IsFolder ? "夹" : string.Empty);
            return stringBuilder.ToString();
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
}
