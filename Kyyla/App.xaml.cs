﻿using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Kyyla.Model;
using ReactiveUI;
using Serilog;
using Splat;
using ILogger = Serilog.ILogger;

namespace Kyyla
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ArriveWindow _arriveWindow;

        private readonly ILogger _logger;

        public App()
        {
            InitializeLogging();
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
            RegisterDependencies();

            MainWindow = new LeaveWindow();
            _arriveWindow = new ArriveWindow();

            _notifyIcon = CreateNotifyIcon();

            Exit += (sender, args) => Log.CloseAndFlush();

            _logger = Log.Logger.ForContext<App>();
            _logger.Debug("App initialized");
        }

        private static void InitializeLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile("Logs\\kyyla-{Date}.log")
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private static void RegisterDependencies()
        {
            Locator.CurrentMutable.RegisterConstant<IArrivalTimeStore>(new ArrivalTimeStore());
            Locator.CurrentMutable.RegisterConstant<ILoginListener>(new LoginListener());
        }

        private NotifyIcon CreateNotifyIcon()
        {
            var iconHandle = Kyyla.Properties.Resources.icon.GetHicon();
            var notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = Icon.FromHandle(iconHandle)
            };
            notifyIcon.MouseClick += (sender, args) =>
            {
                if (args.Button == MouseButtons.Left)
                {
                    ShowMainWindow();
                }
            };

            // Create context menu
            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            var titleItem = notifyIcon.ContextMenuStrip.Items.Add("Kyylä");
            titleItem.Enabled = false;
            var openItem = notifyIcon.ContextMenuStrip.Items.Add("Open");
            openItem.Click += (sender, args) => ShowMainWindow();
            var exitItem = notifyIcon.ContextMenuStrip.Items.Add("Exit");
            exitItem.Click += (sender, args) => ExitApplication();

            return notifyIcon;
        }

        private void ExitApplication()
        {
            _logger.Debug("Application exiting");

            MainWindow.Close();
            _arriveWindow.Close();
            _notifyIcon.Dispose();

            Shutdown(0);
        }

        private void ShowMainWindow()
        {
            var mainWindow = MainWindow;
            if (mainWindow == null)
            {
                _logger.Warning("MainWindow was null");
                return;
            }

            if (mainWindow.IsVisible)
            {
                mainWindow.Activate();
            }
            else
            {
                if (mainWindow is LeaveWindow lv)
                {
                    lv.ViewModel.SoftResetState();
                }
                _logger.Debug("Showing main window");
                mainWindow.Show();
            }
        }
    }
}
