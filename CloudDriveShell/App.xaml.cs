using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Prism.Logging;
using System.Management;
using System.Diagnostics;
using CloudDriveShell.Infrastructure.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using WebDAVClient.Helpers;

namespace CloudDriveShell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly LoggerAdapter Logger = new LoggerAdapter();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var num = Process.GetProcesses().Count(p => p.ProcessName.Equals(Process.GetCurrentProcess().ProcessName)
                && GetProcessUser(p.Id) == GetProcessUser(Process.GetCurrentProcess().Id));
            if (num > 1)
            {
                MessageBox.Show("云盘已启动！");
                this.Shutdown();
            }

#if (DEBUG)
            RunInDebugMode();
#else
            RunInReleaseMode();
#endif
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private static void RunInDebugMode()
        {
            Microsoft.Practices.EnterpriseLibrary.Logging.Logger.SetLogWriter(new LogWriter(new LoggingConfiguration()));
            CloudDriveShellBootstrapper bootstrapper = new CloudDriveShellBootstrapper();
            bootstrapper.Run();
        }

        private static void RunInReleaseMode()
        {
            IConfigurationSource configurationSource = ConfigurationSourceFactory.Create();

            LogWriterFactory logWriterFactory = new LogWriterFactory(configurationSource);
            Microsoft.Practices.EnterpriseLibrary.Logging.Logger.SetLogWriter(logWriterFactory.Create());

            ExceptionPolicyFactory exceptionFactory = new ExceptionPolicyFactory(configurationSource);
            ExceptionManager manager = exceptionFactory.CreateManager();
            ExceptionPolicy.SetExceptionManager(manager);

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            try
            {
                CloudDriveShellBootstrapper bootstrapper = new CloudDriveShellBootstrapper();
                bootstrapper.Run();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                HandleException(e.Exception);
            }
            e.SetObserved();
        }

        private static void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                HandleException(e.Exception);
            }
            e.Handled = true;
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject != null)
            {
                HandleException(e.ExceptionObject as Exception);
            }
        }

        private static void HandleException(Exception ex)
        {
            if (ex == null)
                return;
            ExceptionPolicy.HandleException(ex, "UIPolicy");
            if (ex is WebDAVException)
            {
                var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
                eventAggregator.GetEvent<GlobalExceptionEvent>().Publish(ex.Message);
            }
            Logger.Log("HandleException", Category.Exception, Priority.High);
        }

        private string GetProcessUser(int pid)
        {
            string user = string.Empty;
            SelectQuery query = new SelectQuery("SELECT * FROM Win32_Process WHERE processID=" + pid);
            ManagementObjectSearcher search = new ManagementObjectSearcher(query);
            try
            {
                foreach (var o in search.Get())
                {
                    var item = (ManagementObject)o;
                    if (item["ExecutablePath"] != null)
                    {
                        ManagementBaseObject inPar = null;
                        ManagementBaseObject outPar = null;
                        inPar = item.GetMethodParameters("GetOwner");
                        outPar = item.InvokeMethod("GetOwner", inPar, null);
                        if (outPar != null) user = outPar["User"].ToString();
                        break;
                    }
                }
            }
            catch
            {
                user = "SYSTEM";
            }
            return user;

        }

    }
}

