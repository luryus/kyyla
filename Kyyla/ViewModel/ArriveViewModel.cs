using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Kyyla.Extensions;
using Kyyla.Model;
using ReactiveUI;
using Serilog;
using Splat;
using ILogger = Serilog.ILogger;

namespace Kyyla.ViewModel
{
    public class ArriveViewModel : ReactiveObject
    {
        private string _arrivalTime = "";

        public string ArrivalTime
        {
            get => _arrivalTime;
            set => this.RaiseAndSetIfChanged(ref _arrivalTime, value);
        }

        private readonly ObservableAsPropertyHelper<bool> _isFormValid;
        public bool IsFormValid => _isFormValid.Value;

        private readonly ObservableAsPropertyHelper<DateTimeOffset> _actualTime;
        private readonly IArrivalTimeStore _store;
        private readonly ILogger _logger;
        public DateTimeOffset ActualTime => _actualTime.Value;

        public ReactiveCommand<Unit, Unit> AcceptArrivalTime { get; }

        public event EventHandler? LoginDetected;
             
        public ArriveViewModel()
        {
            // Current time as first value
            ArrivalTime = DateTimeOffset.Now.ToString("HHmm");

            _logger = Log.Logger.ForContext<ArriveViewModel>();
            _isFormValid = this.WhenAnyValue(x => x.ArrivalTime)
                .Select(x => ParseInputString(x) != DateTimeOffset.MinValue)
                .ToProperty(this, x => x.IsFormValid);
            _actualTime = this.WhenAnyValue(x => x.ArrivalTime)
                .Select(ParseInputString)
                .Where(x => x != DateTimeOffset.MinValue)
                .ToProperty(this, x => x.ActualTime);

            _store = Locator.Current.GetRequiredService<IArrivalTimeStore>();
            var loginListener = Locator.Current.GetRequiredService<ILoginListener>();

            AcceptArrivalTime = ReactiveCommand.CreateFromTask(StoreArrivalTime);

            loginListener.LoginDetected += OnLoginDetected;

            _logger.Debug("ArriveViewModel created");
        }

        internal void TriggerLoginDetected()
        {
            OnLoginDetected(this, EventArgs.Empty);
        }

        private async void OnLoginDetected(object? sender, EventArgs? args)
        {
            _logger.Debug("Got LoginDetected event");
            var nowTime = DateTimeOffset.Now;
            var previousTime = await _store.GetArrivalTimeAsync();

            if (nowTime.Date > previousTime.Date)
            {
                _logger.Debug("LoginDetected event first for day, resetting time");
                // It's a new day, show the window
                ArrivalTime = DateTimeOffset.Now.ToString("HHmm");
                LoginDetected?.Invoke(sender, EventArgs.Empty);
            }
        }

        private async Task StoreArrivalTime()
        {
            _logger.Debug("Storing new arrival time: {ActualTime}", ActualTime);
            await _store.SetArrivalTimeAsync(ActualTime);
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
    }
}
