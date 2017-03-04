using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System.Net;
using System.Net.Mime;
using Microsoft.Practices.ServiceLocation;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Utils;
using CloudDriveShell.Infrastructure.Views;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace CloudDriveShell.CloudDriveContent.ViewModels
{
    [Export]
    class CloudDriveExplorerViewModel : BindableBase
    {
        private bool _isAllSelected;
        public bool IsAllSelected
        {
            get { return this._isAllSelected; }
            set
            {
                SetProperty(ref this._isAllSelected, value);
            }
        }

        private string _messageInfo;
        public string MessageInfo
        {
            get { return this._messageInfo; }
            set
            {
                SetProperty(ref this._messageInfo, value);
            }
        }

        private string _messageExtensionInfo;
        public string MessageExtensionInfo
        {
            get { return this._messageExtensionInfo; }
            set
            {
                SetProperty(ref this._messageExtensionInfo, value);
            }
        }

        private string _messageExtensionInfoTooltip;
        public string MessageExtensionInfoTooltip
        {
            get { return this._messageExtensionInfoTooltip; }
            set
            {
                SetProperty(ref this._messageExtensionInfoTooltip, value);
            }
        }

        private bool _hasRunningTransfers;
        public bool HasRunningTransfers
        {
            get { return this._hasRunningTransfers; }
            set
            {
                SetProperty(ref this._hasRunningTransfers, value);
            }
        }

        private int _transfersProgress;
        public int TransfersProgress
        {
            get { return this._transfersProgress; }
            set
            {
                SetProperty(ref this._transfersProgress, value);
            }
        }


        private ResourceItem _currentSelectResourceItem;
        public ResourceItem CurrentSelectResourceItem
        {
            get { return this._currentSelectResourceItem; }
            set
            {
                SetProperty(ref this._currentSelectResourceItem, value);
            }
        }


        private ResourceItem _currentNavigateResourceItem;

        public ResourceItem CurrentNavigateResourceItem
        {
            get { return this._currentNavigateResourceItem; }
            set
            {
                SetProperty(ref this._currentNavigateResourceItem, value);
            }
        }


        private ResourceItem _lastRenameResourceItem;
        public ResourceItem LastRenameResourceItem
        {
            get { return this._lastRenameResourceItem; }
            set
            {
                SetProperty(ref this._lastRenameResourceItem, value);
            }
        }


        private ObservableCollection<ResourceItem> _lastCutOrCopyResourceItems;

        public ObservableCollection<ResourceItem> LastCutOrCopyResourceItems
        {
            get { return this._lastCutOrCopyResourceItems; }
            set
            {
                SetProperty(ref this._lastCutOrCopyResourceItems, value);
            }
        }

        public bool IsMouseRightClick = false;

        private CutOrCopyEnum _cutOrCopyStatus = CutOrCopyEnum.None;

        public ObservableCollection<string> CurrentPathList { get; set; }

        public ObservableCollection<ResourceItem> ResourceItems { get; set; }

        public DelegateCommand ResourceEditCommand { get; private set; }
        public DelegateCommand ResouceHomeCommand { get; private set; }
        public DelegateCommand ResourceRefreshCommand { get; private set; }
        public DelegateCommand ResouceOpenCommand { get; private set; }
        public DelegateCommand ResourceCreateCommand { get; private set; }
        public DelegateCommand ResourceUploadCommand { get; private set; }
        public DelegateCommand ResourceCopyCommand { get; private set; }
        public DelegateCommand ResourceCutCommand { get; private set; }
        public DelegateCommand ResourcePasteCommand { get; private set; }
        public DelegateCommand ResourceRenameCommand { get; private set; }
        public DelegateCommand TransferListViewCommand { get; private set; }
        public DelegateCommand DeleteSelectedCommand { get; private set; }
        public DelegateCommand DownloadSelectedCommand { get; private set; }
        public DelegateCommand UploadSelectedCommand { get; private set; }

        public DelegateCommand<object> ResouceNavigateCommand { get; private set; }



        public Action<string[]> DropUploadCallbackAction;

        public Action<ResourceItem> RenameCallbackAction;

        public Action<bool, bool, ResourceItem> ResourceItemClickAction;

        private readonly IEventAggregator _eventAggregator;

        private readonly IWebDavClientService _webDavClientService;

        private readonly ISwitchContentService _switchContentService;

        [ImportingConstructor]
        public CloudDriveExplorerViewModel(IEventAggregator eventAggregator,
           IWebDavClientService webDavClientService, ISwitchContentService switchContentService)
        {
            this._eventAggregator = eventAggregator;
            this._webDavClientService = webDavClientService;
            this._switchContentService = switchContentService;

            this.ResouceNavigateCommand = new DelegateCommand<object>(this.ResouceNavigate, this.CanResouceNavigate);
            this.ResourceEditCommand = new DelegateCommand(this.ResouceEdit, this.CanResouceEdit);
            this.ResouceHomeCommand = new DelegateCommand(this.ResouceHome, this.CanResouceHome);
            this.ResouceOpenCommand = new DelegateCommand(this.ResouceOpen, this.CanResouceOpen);
            this.ResourceCreateCommand = new DelegateCommand(this.ResouceCreate, this.CanResouceCreate);
            this.ResourceUploadCommand = new DelegateCommand(this.ResouceUpload, this.CanResouceUpload);
            this.ResourceCopyCommand = new DelegateCommand(this.ResouceCopy, this.CanResouceCopy);
            this.ResourceCutCommand = new DelegateCommand(this.ResouceCut, this.CanResouceCut);
            this.ResourcePasteCommand = new DelegateCommand(this.ResoucePaste, this.CanResoucePaste);
            this.ResourceRenameCommand = new DelegateCommand(this.ResouceRename, this.CanResouceRename);
            this.ResourceRefreshCommand = new DelegateCommand(this.ResouceRefresh, this.CanResouceRefresh);
            this.DeleteSelectedCommand = new DelegateCommand(this.DeleteSelected, this.CanDeleteSelected);
            this.DownloadSelectedCommand = new DelegateCommand(this.DownloadSelected, this.CanDownloadSelected);
            this.UploadSelectedCommand = new DelegateCommand(this.UploadSelected, this.CanUploadSelected);
            this.TransferListViewCommand = new DelegateCommand(this.TransferList, this.CanTransferList);

            this.PropertyChanged += OnPropertyChanged;
            this.RenameCallbackAction = RenameCallbackFunction;
            this.ResourceItemClickAction = ResourceItemClickFunction;
            this.DropUploadCallbackAction = DropUploadCallbackFunction;

            this.ResourceItems = new ObservableCollection<ResourceItem>();
            this.CurrentPathList = new ObservableCollection<string>();
            this.LastCutOrCopyResourceItems = new ObservableCollection<ResourceItem>();
            this.LastCutOrCopyResourceItems.CollectionChanged += LastCutOrCopyResourceItems_CollectionChanged;
            this._eventAggregator.GetEvent<RefreshEvent>().Subscribe(path =>
            {
                if (path == null)
                    path = this.CurrentNavigateResourceItem != null ? this.CurrentNavigateResourceItem.ItemHref : WebDavConstant.RootPath;
                this.RefreshCurrentResource(path);
            });
            this._eventAggregator.GetEvent<PasteFileEvent>().Subscribe(files =>
            {
                if (files != null)
                    this.DropUploadCallbackFunction(files);
            });
            this._eventAggregator.GetEvent<PasteResourceEvent>().Subscribe(() =>
            {
                //Ctrl V Just Need Current Folder
                this.ResoucePasteAction(true);
            });
            this._eventAggregator.GetEvent<TransferStatusEvent>().Subscribe(process =>
            {
                this.TransfersProgress = process;
                this.HasRunningTransfers = process < 100;
                if (!this.HasRunningTransfers)
                    this.RefreshCurrentResource();
            });
            this._eventAggregator.GetEvent<GlobalExceptionEvent>().Subscribe((message) =>
            {
                this.NotifyMessageInfo(string.Format("操作异常：{0}", message));
            });
            this._eventAggregator.GetEvent<CreateFolderStatusEvent>().Subscribe(CreateFolderCallback);
            this.RefreshCurrentResource();
        }


        #region PropertyChanged
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "IsAllSelected")
            {
                foreach (var resourceItem in ResourceItems)
                {
                    resourceItem.IsSelected = this.IsAllSelected;
                }
                RaiseAllCanExecuteChanged();
            }

            if (propertyChangedEventArgs.PropertyName == "CurrentSelectResourceItem")
            {
                if (this.CurrentSelectResourceItem == null)
                {
                    foreach (var resourceItem in this.ResourceItems)
                    {
                        if (resourceItem.IsEdit)
                            resourceItem.IsEdit = false;
                        if (resourceItem.IsSelected)
                            resourceItem.IsSelected = false;
                    }
                }

                RaiseAllCanExecuteChanged();
            }
        }


        void resourceItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                RaiseAllCanExecuteChanged();
            }
        }

        void LastCutOrCopyResourceItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var resourceItem in this.ResourceItems)
            {
                resourceItem.IsCopyOrCut = this.LastCutOrCopyResourceItems.Any(cutOrCopyItem => cutOrCopyItem.ItemHref == resourceItem.ItemHref);
            }
        }

        #endregion

        #region Method

        private void RaiseAllCanExecuteChanged()
        {
            this.ResourceEditCommand.RaiseCanExecuteChanged();
            this.ResouceNavigateCommand.RaiseCanExecuteChanged();
            this.ResouceHomeCommand.RaiseCanExecuteChanged();
            this.ResouceOpenCommand.RaiseCanExecuteChanged();
            this.ResourceCreateCommand.RaiseCanExecuteChanged();
            this.ResourceUploadCommand.RaiseCanExecuteChanged();
            this.ResourceCopyCommand.RaiseCanExecuteChanged();
            this.ResourceCutCommand.RaiseCanExecuteChanged();
            this.ResourcePasteCommand.RaiseCanExecuteChanged();
            this.ResourceRenameCommand.RaiseCanExecuteChanged();
            this.ResourceRefreshCommand.RaiseCanExecuteChanged();
            this.DeleteSelectedCommand.RaiseCanExecuteChanged();
            this.DownloadSelectedCommand.RaiseCanExecuteChanged();
            this.UploadSelectedCommand.RaiseCanExecuteChanged();
            this.TransferListViewCommand.RaiseCanExecuteChanged();
        }


        private void ResourceItemClickFunction(bool isCtrl, bool isRight, ResourceItem clickedResourceItem)
        {
            var preSelectResourceItem = this.CurrentSelectResourceItem;
            if (isCtrl)
            {
                clickedResourceItem.IsSelected = !clickedResourceItem.IsSelected;
            }
            else
            {
                if (isRight)
                {
                    if (preSelectResourceItem != clickedResourceItem)
                    {
                        clickedResourceItem.IsSelected = true;
                    }
                }
                else
                {
                    foreach (var resourceItem in this.ResourceItems)
                    {
                        resourceItem.IsSelected = resourceItem == clickedResourceItem;
                    }
                }
            }
        }



        private void RefreshCurrentResource()
        {
            var path = this.CurrentNavigateResourceItem != null ? this.CurrentNavigateResourceItem.ItemHref : WebDavConstant.RootPath;
            RefreshCurrentResource(path);
        }

        private async void RefreshCurrentResource(string itemHref)
        {
            this.NotifyMessageInfo("正在刷新文件夹，请稍后...");
            this.IsAllSelected = false;
            this.CurrentSelectResourceItem = null;
            if (itemHref == null)
            {
                this.CurrentNavigateResourceItem = null;
                var resourceItems = await this._webDavClientService.GetList(WebDavConstant.RootPath);
                this.ResourceItems.Clear();
                resourceItems.ForEach(this.ResourceItems.Add);
                foreach (var resourceItem in this.ResourceItems)
                {
                    resourceItem.PropertyChanged += resourceItem_PropertyChanged;
                }
                this.CurrentPathList.Clear();
            }
            else
            {
                this.CurrentNavigateResourceItem = await this._webDavClientService.GetFolder(itemHref);
                var resourceItems = await this._webDavClientService.GetList(itemHref);
                this.ResourceItems.Clear();
                resourceItems.ForEach(this.ResourceItems.Add);
                foreach (var resourceItem in this.ResourceItems)
                {
                    resourceItem.PropertyChanged += resourceItem_PropertyChanged;
                }
                this.CurrentPathList.Clear();

                var pathArray = itemHref.Split('/');
                var pathList = new List<string>();
                for (int i = 0; i < pathArray.Length; i++)
                {
                    var pathStr = string.Empty;
                    for (int j = 0; j < i; j++)
                    {
                        pathStr += pathArray[j] + "/";
                    }
                    pathList.Add(pathStr);
                }

                var index = pathList.IndexOf(WebDavConstant.RootPath);
                pathList.Skip(index + 1).ToList().ForEach(this.CurrentPathList.Add);
            }

            //For Update IsCopyOrCut Status
            foreach (var resourceItem in this.ResourceItems)
            {
                resourceItem.IsCopyOrCut = this.LastCutOrCopyResourceItems.Any(cutOrCopyItem => cutOrCopyItem.ItemHref == resourceItem.ItemHref);
            }

            NotifyMessageInfo(string.Empty);
            RaiseAllCanExecuteChanged();
        }


        private void NotifyMessageInfo(string actionInfo)
        {
            this.MessageInfo = string.Format("{0}个文件夹，{1}个文件    {2}",
                this.ResourceItems.Count(item => item.IsFolder),
                this.ResourceItems.Count(item => !item.IsFolder),
                actionInfo);
        }

        private async void RenameCallbackFunction(ResourceItem item)
        {
            if (this.LastRenameResourceItem != null && item != null)
            {
                var lastItem = this.LastRenameResourceItem;
                item.IsEdit = false;
                if (lastItem.ItemName != item.ItemName && !string.IsNullOrEmpty(item.ItemName.Trim()))
                {
                    if (this.ResourceItems.Where(resourceItem => resourceItem != item).All(resourceItem => resourceItem.ItemName != item.ItemName))
                    {
                        var oldHref = WebUtility.UrlDecode(lastItem.ItemHref);
                        var newHref = WebUtility.UrlDecode(string.Format("{0}/{1}/", item.ItemHref.TrimEnd('/').Substring(0, item.ItemHref.TrimEnd('/').LastIndexOf('/')), item.ItemName.Trim()));
                        this.NotifyMessageInfo("正在重命名文件，请稍后...");
                        await this._webDavClientService.RenameItem(item, oldHref, newHref);
                    }
                    else
                    {
                        var dialogView = ServiceLocator.Current.GetInstance<IDialogView>(RegionNames.DialogWindowView);
                        dialogView.ShowDialog(DialogEnum.Ok, "提示", "指定文件夹中已存在同名文件(夹)！");
                    }
                }
            }
            this.LastRenameResourceItem = null;
            RefreshCurrentResource();
        }

        private void DropUploadCallbackFunction(string[] files)
        {
            if (files.Length == 0) return;
            var view = ServiceLocator.Current.GetInstance<IContentView>(RegionNames.CloudDriveTransferView);
            if (view == null) return;
            this.NotifyMessageInfo("正在上传文件，请稍后...");
            this._eventAggregator.GetEvent<PubSubEvent<TransferActionInfo>>().Publish(new TransferActionInfo()
            {
                UploadFileList = files,
                TargetPath = this.CurrentNavigateResourceItem == null ? WebDavConstant.RootPath : WebUtility.UrlDecode(this.CurrentNavigateResourceItem.ItemHref),
                WorkingType = WorkingTypeEnum.Upload
            });
            //TransferList();
        }
        #endregion

        #region CommandMethod


        private bool CanResouceRefresh()
        {
            return true;
        }

        private void ResouceRefresh()
        {
            this.RefreshCurrentResource();
        }


        private void ResouceEdit()
        {
            if (this.CurrentSelectResourceItem == null || this.CurrentSelectResourceItem.IsFolder) return;
            var tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            if (!System.IO.Directory.Exists(tempFolder))
                System.IO.Directory.CreateDirectory(tempFolder);
            var view = ServiceLocator.Current.GetInstance<IContentView>(RegionNames.CloudDriveTransferView);
            if (view == null) return;
            this.NotifyMessageInfo("正在下载文件，请稍后...");
            this._eventAggregator.GetEvent<PubSubEvent<EditorActionInfo>>().Publish(new EditorActionInfo()
            {
                DownloadFile = this.CurrentSelectResourceItem,
                TargetPath = tempFolder,
            });
            //TransferList();
        }

        private bool CanResouceEdit()
        {
            return this.CurrentSelectResourceItem != null && !this.CurrentSelectResourceItem.IsFolder;
        }

        private bool CanResouceNavigate(object arg)
        {
            return true;
        }

        private void ResouceNavigate(object obj)
        {
            if (obj == null) return;
            if (obj is string)
            {
                RefreshCurrentResource(obj.ToString());
            }
            else if (obj is ResourceItem)
            {
                var resourceItem = (ResourceItem)obj;
                if (resourceItem.IsFolder && !resourceItem.IsEdit)
                {
                    RefreshCurrentResource(resourceItem.ItemHref);
                }
            }
        }

        private void ResouceHome()
        {
            this.CurrentNavigateResourceItem = this.CurrentSelectResourceItem = null;
            RefreshCurrentResource();
        }

        private bool CanResouceHome()
        {
            return true;
        }

        private bool CanResouceOpen()
        {
            return this.CurrentSelectResourceItem != null && this.CurrentSelectResourceItem.IsFolder;
        }

        private void ResouceOpen()
        {
            if (this.CurrentSelectResourceItem != null && this.CurrentSelectResourceItem.IsFolder && !this.CurrentSelectResourceItem.IsEdit)
            {
                this.CurrentNavigateResourceItem = this.CurrentSelectResourceItem;
                RefreshCurrentResource();
            }
            if (this.CurrentSelectResourceItem != null && !this.CurrentSelectResourceItem.IsFolder && !this.CurrentSelectResourceItem.IsEdit)
            {
                ResouceEdit();
            }
        }

        private bool CanResouceCreate()
        {
            return this.CurrentSelectResourceItem == null;
        }

        private void ResouceCreate()
        {
            var view = ServiceLocator.Current.GetInstance<IContentView>(RegionNames.CloudDriveCreateFolderView) as Window;
            if (view == null) return;
            view.Owner = Application.Current.MainWindow;
            view.ShowDialog();
        }

        private async void CreateFolderCallback(string folderName)
        {
            if (string.IsNullOrEmpty(folderName)) return;
            ResourceItem targetFolder = this.CurrentNavigateResourceItem;
            this.NotifyMessageInfo("正在创建文件夹，请稍后...");
            await this._webDavClientService.CreateDir(WebUtility.UrlDecode(targetFolder == null ? WebDavConstant.RootPath : targetFolder.ItemHref), WebUtility.UrlDecode(folderName));
            RefreshCurrentResource();
        }


        private bool CanResouceUpload()
        {
            return this.CurrentSelectResourceItem == null || this.CurrentSelectResourceItem.IsFolder;
        }

        private void ResouceUpload()
        {
            var view = ServiceLocator.Current.GetInstance<IContentView>(RegionNames.CloudDriveTransferView);
            if (view == null) return;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All files (*.*)|*.*",
                InitialDirectory = KnownFolders.GetPath(KnownFolder.Downloads)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var targetResource = this.CurrentSelectResourceItem == null ? this.CurrentNavigateResourceItem : (
                    this.CurrentSelectResourceItem.IsFolder ? this.CurrentSelectResourceItem : this.CurrentNavigateResourceItem
                    );
                this.NotifyMessageInfo("正在上传文件，请稍后...");
                this._eventAggregator.GetEvent<PubSubEvent<TransferActionInfo>>().Publish(new TransferActionInfo()
                {
                    UploadFileList = openFileDialog.FileNames,
                    TargetPath = targetResource == null ? WebDavConstant.RootPath : WebUtility.UrlDecode(targetResource.ItemHref),
                    WorkingType = WorkingTypeEnum.Upload
                });
                //TransferList();
            }
        }


        private bool CanResouceRename()
        {
            return this.CurrentSelectResourceItem != null;
        }

        private void ResouceRename()
        {
            if (this.CurrentSelectResourceItem != null)
            {
                this.LastRenameResourceItem = this.CurrentSelectResourceItem.CloneJson();
                this.CurrentSelectResourceItem.IsEdit = true;
            }
        }

        private bool CanResoucePaste()
        {
            return this._cutOrCopyStatus != CutOrCopyEnum.None
                && this.LastCutOrCopyResourceItems.Any()
                && (this.CurrentSelectResourceItem == null || this.CurrentSelectResourceItem.IsFolder);
        }

        private void ResoucePaste()
        {
            //Right ContextMenu Has Selected Folder
            ResoucePasteAction(false);
        }

        private async void ResoucePasteAction(bool isCtrlV)
        {
            ResourceItem targetFolderResourceItem;
            if (isCtrlV)
            {
                targetFolderResourceItem = this.CurrentNavigateResourceItem;
            }
            else
            {
                if (this.CurrentSelectResourceItem == null)
                {
                    targetFolderResourceItem = this.CurrentNavigateResourceItem;
                }
                else
                {
                    targetFolderResourceItem = this.CurrentSelectResourceItem.IsFolder ? this.CurrentSelectResourceItem : this.CurrentNavigateResourceItem;
                }
            }
            foreach (var lastItem in this.LastCutOrCopyResourceItems)
            {
                var oldHref = WebUtility.UrlDecode(lastItem.ItemHref).TrimEnd('/');
                string newHref;
                if (targetFolderResourceItem == null)
                {
                    newHref = WebUtility.UrlDecode(string.Format("{0}/{1}/", WebDavConstant.RootPath.TrimEnd('/'), lastItem.ItemName)).TrimEnd('/');
                }
                else
                {
                    if (targetFolderResourceItem.IsFolder)
                    {
                        newHref = WebUtility.UrlDecode(string.Format("{0}/{1}/", targetFolderResourceItem.ItemHref.TrimEnd('/'),
                                lastItem.ItemName)).TrimEnd('/');
                    }
                    else
                    {
                        newHref = WebUtility.UrlDecode(string.Format("{0}/{1}/", targetFolderResourceItem.ParentHref.TrimEnd('/'),
                              lastItem.ItemName)).TrimEnd('/');
                    }
                }
                if (oldHref != newHref)
                {
                    var isExistSameNameItem = this.ResourceItems.All(item => item.ItemName != lastItem.ItemName);
                    if (this.CurrentNavigateResourceItem != targetFolderResourceItem)
                    {
                        var selectedFolderItemList = await this._webDavClientService.GetList(targetFolderResourceItem == null ? WebDavConstant.RootPath : targetFolderResourceItem.ItemHref);
                        isExistSameNameItem = selectedFolderItemList.All(item => item.ItemName != lastItem.ItemName);
                    }
                    if (isExistSameNameItem)
                    {
                        if (lastItem.IsFolder && string.Format("{0}/", newHref).IndexOf(string.Format("{0}/", oldHref), StringComparison.CurrentCulture) == 0)
                        {
                            var dialogView = ServiceLocator.Current.GetInstance<IDialogView>(RegionNames.DialogWindowView);
                            dialogView.ShowDialog(DialogEnum.Ok, "提示", "指定文件夹是移动/复制文件夹的子文件夹！");
                        }
                        else
                        {
                            if (this._cutOrCopyStatus == CutOrCopyEnum.Cut)
                            {
                                this.NotifyMessageInfo(string.Format("正在移动资源：[{0}]，请稍后...", lastItem.ItemName));
                                await this._webDavClientService.MoveItem(lastItem, oldHref, newHref);
                            }
                            if (this._cutOrCopyStatus == CutOrCopyEnum.Copy)
                            {
                                this.NotifyMessageInfo(string.Format("正在复制资源：[{0}]，请稍后...", lastItem.ItemName));
                                await this._webDavClientService.CopyItem(lastItem, oldHref, newHref);
                            }
                        }
                    }
                    else
                    {
                        var dialogView = ServiceLocator.Current.GetInstance<IDialogView>(RegionNames.DialogWindowView);
                        dialogView.ShowDialog(DialogEnum.Ok, "提示", string.Format("指定文件夹中已存在同名文件{0}:[{1}]！",
                            lastItem.IsFolder ? "夹" : "", lastItem.ItemName));
                    }
                }
                else
                {
                    var dialogView = ServiceLocator.Current.GetInstance<IDialogView>(RegionNames.DialogWindowView);
                    dialogView.ShowDialog(DialogEnum.Ok, "提示", string.Format("不能{0}文件{1}:[{2}]至原目录！",
                        this._cutOrCopyStatus == CutOrCopyEnum.Cut ? "移动" : "复制", lastItem.IsFolder ? "夹" : "", lastItem.ItemName));
                }
            }
            this._cutOrCopyStatus = CutOrCopyEnum.None;
            this.MessageExtensionInfo = string.Empty;
            this.MessageExtensionInfoTooltip = string.Empty;
            this.LastCutOrCopyResourceItems.Clear();
            RefreshCurrentResource();
        }

        private bool CanResouceCut()
        {
            return this.ResourceItems.Any(item => item.IsSelected);
        }

        private void ResouceCut()
        {
            this.LastCutOrCopyResourceItems.Clear();
            this.MessageExtensionInfo = this.MessageExtensionInfoTooltip = string.Empty;
            if (ResourceItems.Any(item => item.IsSelected))
            {
                var tootipInfo = new StringBuilder();
                foreach (var resourceItem in this.ResourceItems)
                {
                    if (resourceItem.IsSelected)
                    {
                        this.LastCutOrCopyResourceItems.Add(resourceItem.CloneJson());
                        resourceItem.IsSelected = false;
                        tootipInfo.AppendLine(resourceItem.ItemName);
                    }
                }
                this._cutOrCopyStatus = CutOrCopyEnum.Cut;
                System.Windows.Forms.Clipboard.SetData(CommonConstant.ClipboardCopyResourceItemsDataFormats, "移动");
                this.MessageExtensionInfo = string.Format("选定{0}个移动的资源", this.LastCutOrCopyResourceItems.Count);
                this.MessageExtensionInfoTooltip = tootipInfo.ToString().TrimEnd(Environment.NewLine.ToCharArray());
            }
        }

        private bool CanResouceCopy()
        {
            return this.ResourceItems.Any(item => item.IsSelected);
        }

        private void ResouceCopy()
        {
            this.LastCutOrCopyResourceItems.Clear();
            this.MessageExtensionInfo = this.MessageExtensionInfoTooltip = string.Empty;
            if (ResourceItems.Any(item => item.IsSelected))
            {
                var tootipInfo = new StringBuilder();
                foreach (var resourceItem in this.ResourceItems)
                {
                    if (resourceItem.IsSelected)
                    {
                        this.LastCutOrCopyResourceItems.Add(resourceItem.CloneJson());
                        resourceItem.IsSelected = false;
                        tootipInfo.AppendLine(resourceItem.ItemName);
                    }
                }
                this._cutOrCopyStatus = CutOrCopyEnum.Copy;
                System.Windows.Forms.Clipboard.SetData(CommonConstant.ClipboardCopyResourceItemsDataFormats, "复制");
                this.MessageExtensionInfo = string.Format("选定{0}个复制的资源", this.LastCutOrCopyResourceItems.Count);
                this.MessageExtensionInfoTooltip = tootipInfo.ToString().TrimEnd(Environment.NewLine.ToCharArray());
            }
        }

        private bool CanTransferList()
        {
            return true;
        }

        private void TransferList()
        {
            this._switchContentService.SwitchContentView(RegionNames.ActionCloudDriveRightRegion, RegionNames.CloudDriveTransferView);
        }


        private async void DeleteSelected()
        {
            var dialogView = ServiceLocator.Current.GetInstance<IDialogView>(RegionNames.DialogWindowView);
            if (!dialogView.ShowDialog(DialogEnum.OkOrCanel, "请确认",
                string.Format("是否删除选定资源({0}个)？", this.ResourceItems.Count(item => item.IsSelected)))) return;
            this.NotifyMessageInfo("正在删除文件(夹)，请稍后...");
            foreach (var resourceItem in this.ResourceItems)
            {
                if (resourceItem.IsSelected)
                    await this._webDavClientService.DeleteItem(resourceItem);
            }
            RefreshCurrentResource();
        }

        private bool CanDeleteSelected()
        {
            return this.ResourceItems.Any(item => item.IsSelected);
        }

        private bool CanUploadSelected()
        {
            return true;
        }

        private void UploadSelected()
        {
            this.ResouceUpload();
        }

        private bool CanDownloadSelected()
        {
            return this.ResourceItems.Any(item => item.IsSelected && !item.IsFolder);
        }

        private void DownloadSelected()
        {
            var view = ServiceLocator.Current.GetInstance<IContentView>(RegionNames.CloudDriveTransferView);
            if (view == null) return;
            FolderBrowserDialog openBrowserDialog = new FolderBrowserDialog { ShowNewFolderButton = true, SelectedPath = KnownFolders.GetPath(KnownFolder.Downloads) };
            DialogResult result = openBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.NotifyMessageInfo("正在下载文件，请稍后...");
                var downloadItems = ResourceItems.Where(resourceItem => resourceItem.IsSelected && !resourceItem.IsFolder).ToList();
                this._eventAggregator.GetEvent<PubSubEvent<TransferActionInfo>>().Publish(new TransferActionInfo()
                {
                    DownloadFileList = downloadItems,
                    TargetPath = openBrowserDialog.SelectedPath,
                    WorkingType = WorkingTypeEnum.Download
                });
                //TransferList();
            }
        }

        #endregion

    }
    public class RefreshEvent : PubSubEvent<string>
    {
    }

    public class TransferStatusEvent : PubSubEvent<int>
    {
    }

    enum CutOrCopyEnum
    {
        None,
        Cut,
        Copy
    }
}
