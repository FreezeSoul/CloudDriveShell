using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace CloudDriveShell.Infrastructure.Views
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class DialogWindowViewModel : BindableBase
    {

        private string _dialogTitle;
        public string DialogTitle
        {
            get { return this._dialogTitle; }
            set
            {
                SetProperty(ref this._dialogTitle, value);
            }
        }

        private string _dialogMessage;
        public string DialogMessage
        {
            get { return this._dialogMessage; }
            set
            {
                SetProperty(ref this._dialogMessage, value);
            }
        }

        public event EventHandler<bool> RequestClose;

        public DelegateCommand OkCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        private readonly IRegionManager _regionManager;

        private readonly IEventAggregator _eventAggregator;
        [ImportingConstructor]
        public DialogWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            this._regionManager = regionManager;
            this._eventAggregator = eventAggregator;
            this.OkCommand = new DelegateCommand(this.Ok, this.CanOk);
            this.CancelCommand = new DelegateCommand(this.Cancel, this.CanCancel);
        }


        private bool CanOk()
        {
            return true;
        }

        private void Ok()
        {
            if (RequestClose != null)
            {
                RequestClose(this, true);
            }
        }


        private bool CanCancel()
        {
            return true;
        }


        private void Cancel()
        {
            if (RequestClose != null)
            {
                RequestClose(this, false);
            }
        }
    }
}
