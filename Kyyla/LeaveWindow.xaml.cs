#nullable disable

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls.Primitives;
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
            Closing += OnClosing;
            InitializeComponent();
            ViewModel = new LeaveViewModel();

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

                this.BindCommand(ViewModel, x => x.SaveArrivalTime, x => x.StoreArrivalTimeButton).DisposeWith(disposables);

                AbsenceListButton.Events().Click
                    .Subscribe(ev =>
                    {
                        ShowAbsenceListDialog();
                    });
            });
        }

        private void ShowAbsenceListDialog()
        {
            var absenceWindow = new AbsenceWindow();
            Debug.Assert(absenceWindow.ViewModel != null);

            absenceWindow.ViewModel.AbsenceTimeChange += (sender, span) =>
            {
                if (ViewModel == null)
                {
                    return;
                }
                
                var minutes = (int) span.TotalMinutes;
                if (!int.TryParse(ViewModel.OtherAbsenceInput, out var previousAbsence))
                {
                    previousAbsence = 0;
                }

                var newAbsence = previousAbsence + minutes;
                if (newAbsence < 0)
                {
                    newAbsence = 0;
                }

                ViewModel.OtherAbsenceInput = newAbsence.ToString();
            };
            
            absenceWindow.ShowDialog();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
