using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Interfaces;
using CloudDriveShell.Infrastructure.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace CloudDriveShell.TopNavigation.ViewModels
{


    [Export(typeof(TopNavigationViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TopNavigationViewModel : BindableBase
    {
        public AccountInfo AccountInfo
        {
            get { return CurrentAccountInfo.Instance; }
        }

        private IList<NavigateMenu> _navigateMenus;
        public IList<NavigateMenu> NavigateMenus
        {
            get
            {
                if (this._navigateMenus == null)
                {
                    this._navigateMenus = InitNavigateMenus();
                }
                return this._navigateMenus;
            }

        }

        private NavigateMenu _selectedNavigateMenu;
        public NavigateMenu SelectedNavigateMenu
        {
            get
            {
                if (this._selectedNavigateMenu == null && this._navigateMenus != null)
                {
                    this._selectedNavigateMenu = this._navigateMenus.FirstOrDefault();
                }
                return this._selectedNavigateMenu;
            }
            set
            {
                SetProperty(ref this._selectedNavigateMenu, value);
            }
        }


        private readonly IRegionManager _regionManager;

        private readonly IEventAggregator _eventAggregator;

        private readonly ISwitchContentService _switchContentService;


        [ImportMany(typeof(INavigateMeta))]
        #pragma warning disable 649
        private IEnumerable<Lazy<INavigateMeta>> _navigateMetaLazies;
        #pragma warning restore 649


        public ICommand SelectedCommand { set; get; }

        [ImportingConstructor]
        public TopNavigationViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ISwitchContentService switchContentService)
        {
            this._regionManager = regionManager;
            this._eventAggregator = eventAggregator;
            this._switchContentService = switchContentService;
            this.SelectedCommand = new DelegateCommand(this.SelectedChanged);

            this._switchContentService.SwitchContentView(RegionNames.CloudDriveContentView);
        }

        private void SelectedChanged()
        {
            this._switchContentService.SwitchContentView(this.SelectedNavigateMenu.RegionViewName);
        }

        private IList<NavigateMenu> InitNavigateMenus()
        {
            var menus = new List<NavigateMenu>();
            if (_navigateMetaLazies != null)
            {
                foreach (Lazy<INavigateMeta> navigateMeta in _navigateMetaLazies)
                {
                    menus.Add(navigateMeta.Value.LoadNavigateMenu());
                }
            }
            return menus.OrderBy(o => o.SortIndex).ToList(); ;
        }
    }
}
