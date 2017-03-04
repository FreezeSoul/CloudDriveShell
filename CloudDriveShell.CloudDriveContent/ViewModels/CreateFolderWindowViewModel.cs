using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudDriveShell.Infrastructure.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace CloudDriveShell.CloudDriveContent.ViewModels
{
    [Export]
    class CreateFolderWindowViewModel : BindableBase
    {

        private string _folderName;

        public string FolderName
        {
            get { return this._folderName; }
            set
            {
                SetProperty(ref this._folderName, FileHelper.RemovePathUnSupportChart(value));
            }
        }

        public event EventHandler RequestClose;

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CloseCommand { get; private set; }

        private readonly IRegionManager _regionManager;

        private readonly IEventAggregator _eventAggregator;

        [ImportingConstructor]
        public CreateFolderWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            this._regionManager = regionManager;
            this._eventAggregator = eventAggregator;
            this.SaveCommand = new DelegateCommand(this.Save, this.CanSave);
            this.CloseCommand = new DelegateCommand(this.Close, this.CanClose);
            this.PropertyChanged += OnPropertyChanged;
        }

        private bool CanClose()
        {
            return true;
        }

        private void Close()
        {
            if (RequestClose != null)
            {
                RequestClose(this, null);
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrEmpty(FolderName);
        }

        private void Save()
        {
            if (RequestClose != null)
            {
                RequestClose(this, null);
                if (FolderName != null && !string.IsNullOrEmpty(FolderName.Trim()))
                    this._eventAggregator.GetEvent<CreateFolderStatusEvent>().Publish(FolderName.Trim());
                this.FolderName = string.Empty;
            }
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "FolderName")
            {
                this.SaveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public class CreateFolderStatusEvent : PubSubEvent<string>
    {
    }
}
