using System;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Windows;
using Kyyla.ViewModel;
using ReactiveUI;
using Serilog;

namespace Kyyla
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ArriveWindow : ReactiveWindow<ArriveViewModel>
    {
        private readonly ILogger _logger;

        public ArriveWindow()
        {
            _logger = Log.Logger.ForContext<ArriveWindow>();
            InitializeComponent();
            ViewModel = new ArriveViewModel();

            Closing += OnClosing;
            ViewModel.LoginDetected += ViewModelOnLoginDetected;

            var workArea = SystemParameters.WorkArea;
            Top = workArea.Bottom - Height;
            Left = workArea.Right - Width;

            this.WhenActivated(disposables =>
            {
                _logger.Debug("Activated ArriveWindow");
                this.Bind(ViewModel, vm => vm.ArrivalTime, v => v.ArrivalTimeTextBox.Text).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.IsFormValid, v => v.OkButton.IsEnabled).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.ActualTime, v => v.TestLabel.Content).DisposeWith(disposables);

                this.BindCommand(ViewModel, vm => vm.AcceptArrivalTime, v => v.OkButton).DisposeWith(disposables);

                ViewModel.AcceptArrivalTime.Subscribe(_ => Hide()).DisposeWith(disposables);

                ArrivalTimeTextBox.SelectAll();
                ArrivalTimeTextBox.Focus();
            });

            RxApp.MainThreadScheduler.Schedule(() => ViewModel.TriggerLoginDetected());
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void ViewModelOnLoginDetected(object sender, EventArgs e)
        {
            if (IsVisible)
            {
                return;
            }

            Show();
            Activate();
        }
    }
}
