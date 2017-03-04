

using System;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using CloudDriveShell.CloudDriveContent;
using CloudDriveShell.CloudOtherContent;
using CloudDriveShell.CloudSettingContent;
using CloudDriveShell.Infrastructure;
using CloudDriveShell.Infrastructure.Behaviors;
using CloudDriveShell.Infrastructure.Extension;
using CloudDriveShell.TopNavigation;
using CloudDriveShell.LoginView;
using CloudDriveShell.LoginView.Views;
using Prism.Logging;
using Prism.Mef;
using Prism.Regions;
using Transitionals.Controls;

namespace CloudDriveShell
{
    public class CloudDriveShellBootstrapper : MefBootstrapper
    {

        private readonly LoggerAdapter _logger = new LoggerAdapter();

        protected override ILoggerFacade CreateLogger()
        {
            return _logger;
        }

        protected override void ConfigureAggregateCatalog()
        {
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CloudDriveShellBootstrapper).Assembly));
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(RegionNames).Assembly));
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(LoginViewModule).Assembly));
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(TopNavigationModule).Assembly));
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CloudDriveContentModule).Assembly));
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CloudSettingContentModule).Assembly));
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CloudOtherContentModule).Assembly));
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (Shell)this.Shell;
            var loginView = this.Container.GetExportedValue<Login>();
            loginView.Icon = ((Shell)this.Shell).Icon.Clone();
            if (loginView.ShowDialog() != true)
            {
                Environment.Exit(1);
            }
            Application.Current.MainWindow.Show();
        }
        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            RegionAdapterMappings mappings = base.ConfigureRegionAdapterMappings();
            mappings.RegisterMapping(typeof(TransitionElement), Container.GetExportedValue<TransitionElementAdaptor>());
            return mappings;
        }
        protected override IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
        {
            var factory = base.ConfigureDefaultRegionBehaviors();
            factory.AddIfMissing("AutoPopulateExportedViewsBehavior", typeof(AutoPopulateExportedViewsBehavior));
            return factory;
        }

        protected override DependencyObject CreateShell()
        {
            return this.Container.GetExportedValue<Shell>();
        }
    }
}