using System.ComponentModel;
using System.Reactive.Disposables;
using System.Windows;
using Kyyla.ViewModel;
using ReactiveUI;

namespace Kyyla
{
    /// <summary>
    /// Interaction logic for LeaveWindow.xaml
    /// </summary>
    public partial class LeaveWindow : ReactiveWindow<LeaveViewModel>
    {
        public LeaveWindow()
        {
            InitializeComponent();
            ViewModel = new LeaveViewModel();

            Closing += OnClosing;

            var workingArea = SystemParameters.WorkArea;
            Top = workingArea.Bottom - Height;
            Left = workingArea.Right - Width;

            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.ArrivalTimeInput, v => v.ArrivalTimeTextBox.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.LeaveTimeInput, v => v.LeaveTimeTextBox.Text).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.LunchDurationInput, v => v.LunchDurationTextBox.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.OtherAbsenceInput, v => v.AbsenceDurationTextBox.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.TotalWorkTime, v => v.HourMinutesTotalTimeLabel.Content,
                    dur => $"{dur:%h}h {dur:%m}min").DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.TotalWorkTime, v => v.HoursTotalTimeLabel.Content,
                    dur => $"{dur.TotalHours:F} h");
            });
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
