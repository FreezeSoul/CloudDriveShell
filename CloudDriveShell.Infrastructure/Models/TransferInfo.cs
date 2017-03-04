using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CloudDriveShell.Infrastructure.Models
{
    [Serializable]
    public class TransferInfo : INotifyPropertyChanged
    {
        private string _fileName;

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; this.NotifyPropertyChanged("FileName"); }
        }


        private long _fileSize;

        public long FileSize
        {
            get { return _fileSize; }
            set { _fileSize = value; this.NotifyPropertyChanged("FileSize"); }
        }


        private long _finishSize;

        public long FinishSize
        {
            get { return _finishSize; }
            set { _finishSize = value; this.NotifyPropertyChanged("FinishSize"); }
        }


        private int _progressPercentage;

        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            set { _progressPercentage = value; this.NotifyPropertyChanged("ProgressPercentage"); }
        }


        private bool _startWorking;

        public bool StartWorking
        {
            get { return _startWorking; }
            set { _startWorking = value; this.NotifyPropertyChanged("StartWorking"); }
        }

        private bool _isFailed;
        public bool IsFailed
        {
            get { return _isFailed; }
            set { _isFailed = value; this.NotifyPropertyChanged("IsFailed"); }
        }

        private string _failedInfo;
        public string FailedInfo
        {
            get { return _failedInfo; }
            set { _failedInfo = value; this.NotifyPropertyChanged("FailedInfo"); }
        }


        private DateTime _transferTime;

        public DateTime TransferTime
        {
            get { return _transferTime; }
            set { _transferTime = value; this.NotifyPropertyChanged("TransferTime"); }
        }


        private string _fileLocalPath;

        public string FileLocalPath
        {
            get { return _fileLocalPath; }
            set { _fileLocalPath = value; this.NotifyPropertyChanged("FileLocalPath"); }
        }

        private string _fileCloudPath;

        public string FileCloudPath
        {
            get { return _fileCloudPath; }
            set { _fileCloudPath = value; this.NotifyPropertyChanged("FileCloudPath"); }
        }

        private WorkingTypeEnum _workingType;

        public WorkingTypeEnum WorkingType
        {
            get { return _workingType; }
            set { _workingType = value; this.NotifyPropertyChanged("WorkingType"); }
        }


        private bool _isRealyDoing;
        [XmlIgnore]
        public bool IsRealyDoing
        {
            get { return _isRealyDoing; }
            set { _isRealyDoing = value; this.NotifyPropertyChanged("IsRealyDoing"); }
        }


        [XmlIgnore]
        public bool IsUpload
        {
            get { return _workingType == WorkingTypeEnum.Upload; }
        }

        [XmlIgnore]
        public bool IsDownload
        {
            get { return _workingType == WorkingTypeEnum.Download; }
        }

        [XmlIgnore]
        private TransferTasker _tasker;
        public void SetTasker(TransferTasker tasker)
        {
            this._tasker = tasker;
        }

        public TransferTasker GeTasker()
        {
            return this._tasker;
        }

        public override string ToString()
        {
            return string.Format("双击打开文件所在云盘路径：/{0}{1}Ctrl+双击打开文件本地目录：{2}",
               WebUtility.UrlDecode(this.FileCloudPath).Replace(WebDavConstant.RootPath, string.Empty).TrimEnd('/'), 
               Environment.NewLine, this.FileLocalPath);
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

    public enum WorkingTypeEnum
    {
        Download,
        Upload
    }
}
