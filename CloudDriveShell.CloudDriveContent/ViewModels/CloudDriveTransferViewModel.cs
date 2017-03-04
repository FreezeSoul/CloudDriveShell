using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;
using CloudDriveShell.Infrastructure.Utils;
using CloudDriveShell.Infrastructure.Views;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace CloudDriveShell.CloudDriveContent.ViewModels
{
    [Export]
    class CloudDriveTransferViewModel : BindableBase
    {
        private int _currentRunningTransferCount;

        public ObservableCollection<TransferInfo> TransferInfos { get; set; }

        private readonly IEventAggregator _eventAggregator;

        private readonly IWebDavClientService _webDavClientService;

        private readonly ITransferDataService _transferDataService;

        private readonly ISwitchContentService _switchContentService;

        public DelegateCommand BackCommand { get; private set; }
        public DelegateCommand ClearCommand { get; private set; }
        public DelegateCommand CancelAllCommand { get; private set; }
        public DelegateCommand<TransferInfo> CancelTransferCommand { get; private set; }
        public DelegateCommand<TransferInfo> NavigateItemCommand { get; private set; }

        [ImportingConstructor]
        public CloudDriveTransferViewModel(IEventAggregator eventAggregator,
            IWebDavClientService webDavClientService, ISwitchContentService switchContentService
            , ITransferDataService transferDataService)
        {
            this._eventAggregator = eventAggregator;
            this._webDavClientService = webDavClientService;
            this._switchContentService = switchContentService;
            this._transferDataService = transferDataService;

            this.TransferInfos = new ObservableCollection<TransferInfo>(this._transferDataService.RetriveHistory());

            this.BackCommand = new DelegateCommand(this.Back, this.CanBack);
            this.ClearCommand = new DelegateCommand(this.Clear, this.CanClear);
            this.CancelAllCommand = new DelegateCommand(this.CancelAll, this.CanCancelAll);
            this.NavigateItemCommand = new DelegateCommand<TransferInfo>(this.NavigateItem, this.CanNavigateItem);
            this.CancelTransferCommand = new DelegateCommand<TransferInfo>(this.CancelTransfer, this.CanCancelTransfer);
            this._eventAggregator.GetEvent<PubSubEvent<TransferActionInfo>>().Subscribe(UploadOrDownloadAction);
            this._eventAggregator.GetEvent<PubSubEvent<EditorActionInfo>>().Subscribe(EditorAction);
            this._eventAggregator.GetEvent<WindowClosingEvent>().Subscribe(SaveTransferInfos, ThreadOption.UIThread);
        }

        private void ComputeTransferProcess()
        {
            if (_currentRunningTransferCount == 0)
            {
                this._eventAggregator.GetEvent<TransferStatusEvent>().Publish(100);
            }
            else
            {
                var nowWokingCount = this.TransferInfos.Count(t => t.StartWorking);
                var currentProgress = 100 - nowWokingCount * 100 / _currentRunningTransferCount;
                if (nowWokingCount == 0) currentProgress = 100;
                this._eventAggregator.GetEvent<TransferStatusEvent>().Publish(currentProgress);
            }

        }

        private bool CanCancelAll()
        {
            return true;
        }

        private void CancelAll()
        {
            var dialogView = ServiceLocator.Current.GetInstance<IDialogView>(RegionNames.DialogWindowView);
            if (!dialogView.ShowDialog(DialogEnum.OkOrCanel, "请确认", "是否取消所有任务？")) return;
            foreach (var transferInfo in TransferInfos)
                CancelTransfer(transferInfo);
        }

        private void CancelTransfer(TransferInfo transferInfo)
        {
            var tasker = transferInfo.GeTasker();
            if (tasker == null || !transferInfo.StartWorking) return;
            transferInfo.IsFailed = true;
            transferInfo.StartWorking = false;
            transferInfo.FailedInfo = "操作被取消";
            tasker.Cancel();
        }

        private bool CanCancelTransfer(TransferInfo transferInfo)
        {
            return transferInfo.StartWorking;
        }


        private void SaveTransferInfos(System.ComponentModel.CancelEventArgs obj)
        {
            this._transferDataService.SaveHistory(this.TransferInfos.ToList());
        }

        private void UploadOrDownloadAction(TransferActionInfo transferActionInfo)
        {
            if (transferActionInfo.WorkingType == WorkingTypeEnum.Upload)
            {
                foreach (var fileFullPath in transferActionInfo.UploadFileList)
                {
                    var transferInfo = new TransferInfo()
                    {
                        FileName = Path.GetFileName(fileFullPath),
                        FileSize = new FileInfo(fileFullPath).Length,
                        FileLocalPath = fileFullPath,
                        FileCloudPath = transferActionInfo.TargetPath,
                        FinishSize = 0,
                        ProgressPercentage = 0,
                        StartWorking = false,
                        TransferTime = DateTime.Now,
                        WorkingType = transferActionInfo.WorkingType
                    };
                    this.TransferInfos.Add(transferInfo);
                    new TransferTasker(transferInfo).UploadFile(transferInfo.FileLocalPath,
                        string.Format("{0}/{1}", transferInfo.FileCloudPath.TrimEnd('/'), transferInfo.FileName), transferInfo.FileSize,
                        null,
                        (sender, args) =>
                        {
                            ComputeTransferProcess();
                        });
                }
            }
            else if (transferActionInfo.WorkingType == WorkingTypeEnum.Download)
            {
                foreach (var resourceFile in transferActionInfo.DownloadFileList)
                {
                    var transferInfo = new TransferInfo()
                    {
                        FileName = resourceFile.ItemName,
                        FileSize = resourceFile.ItemSize ?? 0,
                        FileLocalPath = Path.Combine(transferActionInfo.TargetPath, resourceFile.ItemName),
                        FileCloudPath = resourceFile.ItemHref,
                        FinishSize = 0,
                        ProgressPercentage = 0,
                        StartWorking = false,
                        TransferTime = DateTime.Now,
                        WorkingType = transferActionInfo.WorkingType
                    };
                    this.TransferInfos.Add(transferInfo);
                    new TransferTasker(transferInfo).DownloadFile(transferInfo.FileLocalPath, resourceFile.ItemHref,
                        null,
                        (sender, args) =>
                        {
                            ComputeTransferProcess();
                        });
                }
            }
            _currentRunningTransferCount = this.TransferInfos.Count(t => t.StartWorking);
            this._eventAggregator.GetEvent<TransferStatusEvent>().Publish(0);
        }

        private void EditorAction(EditorActionInfo info)
        {
            var transferDownloadInfo = new TransferInfo()
            {
                FileName = info.DownloadFile.ItemName,
                FileSize = info.DownloadFile.ItemSize ?? 0,
                FileLocalPath = Path.Combine(info.TargetPath, info.DownloadFile.ItemName),
                FileCloudPath = info.DownloadFile.ItemHref,
                FinishSize = 0,
                ProgressPercentage = 0,
                StartWorking = false,
                TransferTime = DateTime.Now,
                WorkingType = WorkingTypeEnum.Download
            };
            this.TransferInfos.Add(transferDownloadInfo);
            new TransferTasker(transferDownloadInfo).DownloadFile(transferDownloadInfo.FileLocalPath, info.DownloadFile.ItemHref, null, (obj, e) =>
            {
                ComputeTransferProcess();
                if (transferDownloadInfo.IsFailed) return;
                var dateTime = File.GetLastWriteTime(transferDownloadInfo.FileLocalPath);
                var psi = new ProcessStartInfo(transferDownloadInfo.FileLocalPath) { UseShellExecute = true };
                var process = Process.Start(psi);
                if (process != null) process.WaitForExit();
                var modifyTime = File.GetLastWriteTime(transferDownloadInfo.FileLocalPath);
                if (modifyTime > dateTime)
                {
                    var dialogView = ServiceLocator.Current.GetInstance<IDialogView>(RegionNames.DialogWindowView);
                    if (!dialogView.ShowDialog(DialogEnum.OkOrCanel, "请确认", "文件已改动，是否重新上传至服务器？")) return;
                    var transferUploadInfo = new TransferInfo()
                    {
                        FileName = Path.GetFileName(transferDownloadInfo.FileLocalPath),
                        FileSize = new FileInfo(transferDownloadInfo.FileLocalPath).Length,
                        FileLocalPath = transferDownloadInfo.FileLocalPath,
                        FileCloudPath = transferDownloadInfo.FileCloudPath.Substring(0, transferDownloadInfo.FileCloudPath.LastIndexOf('/')),
                        FinishSize = 0,
                        ProgressPercentage = 0,
                        StartWorking = false,
                        TransferTime = DateTime.Now,
                        WorkingType = WorkingTypeEnum.Upload
                    };
                    this.TransferInfos.Add(transferUploadInfo);
                    new TransferTasker(transferUploadInfo).UploadFile(transferUploadInfo.FileLocalPath, string.Format("{0}/{1}",
                        transferUploadInfo.FileCloudPath.TrimEnd('/'), transferUploadInfo.FileName), transferUploadInfo.FileSize,
                        null,
                        (sender, args) =>
                        {
                            ComputeTransferProcess();
                        });

                    _currentRunningTransferCount = this.TransferInfos.Count(t => t.StartWorking);
                    this._eventAggregator.GetEvent<TransferStatusEvent>().Publish(0);
                }
            });
            _currentRunningTransferCount = this.TransferInfos.Count(t => t.StartWorking);
            this._eventAggregator.GetEvent<TransferStatusEvent>().Publish(0);
        }


        private void Clear()
        {
            var dialogView = ServiceLocator.Current.GetInstance<IDialogView>(RegionNames.DialogWindowView);
            if (!dialogView.ShowDialog(DialogEnum.OkOrCanel, "请确认", "是否清除所有任务记录？")) return;
            foreach (var transferInfo in TransferInfos)
                CancelTransfer(transferInfo);
            this.TransferInfos.Clear();
            this._transferDataService.SaveHistory(this.TransferInfos.ToList());
        }

        private bool CanClear()
        {
            return true;
        }

        private bool CanBack()
        {
            return true;
        }

        private void Back()
        {
            this._eventAggregator.GetEvent<RefreshEvent>().Publish(null);
            this._switchContentService.SwitchContentView(RegionNames.ActionCloudDriveRightRegion, RegionNames.CloudDriveExplorerView);
        }


        private void NavigateItem(TransferInfo transferInfo)
        {
            if (transferInfo == null || transferInfo.StartWorking) return;
            if (transferInfo.WorkingType == WorkingTypeEnum.Download)
            {
                var lastIndex = transferInfo.FileCloudPath.LastIndexOf('/');
                var pathStr = string.Format("{0}/", transferInfo.FileCloudPath.Substring(0, lastIndex).TrimEnd('/'));
                this._eventAggregator.GetEvent<RefreshEvent>().Publish(pathStr);
            }
            else
            {
                this._eventAggregator.GetEvent<RefreshEvent>().Publish(transferInfo.FileCloudPath);
            }
            this._switchContentService.SwitchContentView(RegionNames.ActionCloudDriveRightRegion, RegionNames.CloudDriveExplorerView);
        }

        private bool CanNavigateItem(TransferInfo transferInfo)
        {
            return true;
        }

    }

    public class TransferActionInfo
    {
        public IList<string> UploadFileList { set; get; }

        public IList<ResourceItem> DownloadFileList { set; get; }

        public WorkingTypeEnum WorkingType { set; get; }

        public string TargetPath { set; get; }
    }

    public class EditorActionInfo
    {
        public ResourceItem DownloadFile { set; get; }

        public string TargetPath { set; get; }
    }
}
