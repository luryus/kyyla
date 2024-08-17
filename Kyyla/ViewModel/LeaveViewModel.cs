using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;
using Kyyla.Model;
using ReactiveUI;
using Serilog;
using Splat;

namespace Kyyla.ViewModel
{
    public class LeaveViewModel : ReactiveObject
    {

        private const int DefaultLunchDuration = 30;
        private const int DefaultAbsenceDuration = 0;


        private string _arrivalTimeInput = "";
        private string _leaveTimeInput = "";
        private string _lunchDurationInput = "";
        private string _otherAbsenceInput = "";

        public string ArrivalTimeInput
        {
            get => _arrivalTimeInput;
            set => this.RaiseAndSetIfChanged(ref _arrivalTimeInput, value);
        }
        public string LeaveTimeInput
        {
            get => _leaveTimeInput;
            set => this.RaiseAndSetIfChanged(ref _leaveTimeInput, value);
        }
        public string LunchDurationInput
        {
            get => _lunchDurationInput;
            set => this.RaiseAndSetIfChanged(ref _lunchDurationInput, value);
        }
        public string OtherAbsenceInput
        {
            get => _otherAbsenceInput;
            set => this.RaiseAndSetIfChanged(ref _otherAbsenceInput, value);
        }

        public ReactiveCommand<Unit, Unit> SaveArrivalTime { get; }

        private readonly Serilog.ILogger _logger;
        private readonly ObservableAsPropertyHelper<TimeSpan> _totalWorkTime;
        private readonly IArrivalTimeStore _arrivalStore;
        public TimeSpan TotalWorkTime => _totalWorkTime.Value;

        public LeaveViewModel()
        {

            _logger = Log.Logger.ForContext<ArriveViewModel>();

            _totalWorkTime = this
                .WhenAnyValue(x => x.ArrivalTimeInput, x => x.LeaveTimeInput, x => x.LunchDurationInput, x => x.OtherAbsenceInput)
                .Select(_ => CalculateWorkTime())
                .ToProperty(this, x => x.TotalWorkTime);

            // Defaults
            RxApp.MainThreadScheduler.ScheduleAsync(async (s, ct) => await HardResetState());

            _arrivalStore = Locator.Current.GetService<IArrivalTimeStore>();
            _arrivalStore.ArrivalTimeChanged += (sender, arrivalTime) =>
            {
                RxApp.MainThreadScheduler.ScheduleAsync(async (s, ct) => await HardResetState());
            };

            SaveArrivalTime = ReactiveCommand.CreateFromTask(StoreArrivalTime);
        }

        private async Task HardResetState()
        {
            ArrivalTimeInput = (await _arrivalStore.GetArrivalTimeAsync()).ToString("HHmm");
            LeaveTimeInput = DateTimeOffset.Now.ToString("HHmm");
            LunchDurationInput = DefaultLunchDuration.ToString();
            OtherAbsenceInput = DefaultAbsenceDuration.ToString();
        }

        public void SoftResetState()
        {
            LeaveTimeInput = DateTimeOffset.Now.ToString("HHmm");
        }

        private TimeSpan CalculateWorkTime()
        {
            var arrival = ParseInputString(ArrivalTimeInput);
            var leave = ParseInputString(LeaveTimeInput);

            if (arrival == DateTimeOffset.MinValue || leave == DateTimeOffset.MinValue)
            {
                return TimeSpan.Zero;
            }

            if (!int.TryParse(LunchDurationInput, out var lunch))
            {
                return TimeSpan.Zero;
            }

            if (!int.TryParse(OtherAbsenceInput, out var absence))
            {
                return TimeSpan.Zero;
            }

            return leave - arrival - TimeSpan.FromMinutes(lunch) - TimeSpan.FromMinutes(absence);
        }

        private static DateTimeOffset ParseInputString(string input)
        {
            if (DateTimeOffset.TryParseExact(input, "HHmm", new DateTimeFormatInfo(), DateTimeStyles.AssumeLocal,
                out var res))
            {
                return res;
            }

            return DateTimeOffset.MinValue;
        }

        private async Task StoreArrivalTime()
        {
            var currentArrivalTime = ParseInputString(ArrivalTimeInput);
            if (currentArrivalTime == default)
            {
                return;
            }
            _logger.Debug("Storing new arrival time (manual save): {ActualTime}", currentArrivalTime);
            await _arrivalStore.SetArrivalTimeAsync(currentArrivalTime);
        }

    }

}
