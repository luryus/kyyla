using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using Kyyla.Model;
using Microsoft.EntityFrameworkCore;
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
            // WPF or something has a bug where the memory usage of the process jumps to the sky
            // with some Intel GPU drivers. Force software rendering to mitigate this.
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            
            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            
            InitializeLogging();
            using (var db = new LockEventDbContext())
            {
                db.Database.Migrate();
            }


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
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var logsFolder = Path.Combine(appDataFolder, "Kyyla", "Logs");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Join(logsFolder, "kyyla.log"), rollingInterval: RollingInterval.Day)
                .WriteTo.Debug()
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private static void RegisterDependencies()
        {
            Locator.CurrentMutable.RegisterConstant<IArrivalTimeStore>(new ArrivalTimeStore());
            Locator.CurrentMutable.RegisterConstant<ILoginListener>(new LoginListener());
            Locator.CurrentMutable.RegisterConstant<ILockEventStore>(new LockEventStore());
        }

        private NotifyIcon CreateNotifyIcon()
        {
            // Is Windows setup with Dark or Light theme?
            var lightThemeEnabled = false;
            using (var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                var regValue = (int?) regKey?.GetValue("SystemUsesLightTheme");
                if (regValue is int regValueNonNull && regValueNonNull > 0)
                {
                    lightThemeEnabled = true;
                }
            }

            var iconHandle = lightThemeEnabled ? Kyyla.Properties.Resources.clock : Kyyla.Properties.Resources.clock_white;
            var notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = iconHandle
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
                    lv.ViewModel?.SoftResetState();
                }
                _logger.Debug("Showing main window");
                mainWindow.Show();
            }
        }
    }
}
