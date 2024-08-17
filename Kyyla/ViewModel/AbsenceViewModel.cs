using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using Kyyla.Extensions;
using Kyyla.Model;
using Splat;

namespace Kyyla.ViewModel
{
    public sealed class AbsenceViewModel : ReactiveObject, IActivatableViewModel
    {
        private ReadOnlyObservableCollection<AbsenceRowViewModel>? _rows;
        public ViewModelActivator Activator { get; } = new();

        private readonly ConcurrentDictionary<DateTimeOffset, bool> _checkedStartTimes = new ();

        public event EventHandler<TimeSpan>? AbsenceTimeChange; 
        
        public AbsenceViewModel()
        {            
            var lockEventStore = Locator.Current.GetRequiredService<ILockEventStore>();

            this.WhenActivated(disposables =>
            {
                var viewModels = lockEventStore.CurrentDayLockEvents.Connect()
                    .Transform(x =>
                    {
                        var (start, end) = x;
                        return new AbsenceRowViewModel
                        {
                            StartTime = start, 
                            EndTime = end, 
                            Checked = start != null && _checkedStartTimes.ContainsKey(start.Value)
                        };
                    }, transformOnRefresh: true)
                    .AsObservableList()
                    .DisposeWith(disposables);
                    
                viewModels.Connect()
                    .ObserveOnDispatcher()
                    .Bind(out _rows)
                    .Subscribe()
                    .DisposeWith(disposables);

                viewModels.Connect()
                    .WhenPropertyChanged(x => x.Checked, notifyOnInitialValue: false)
                    .ObserveOnDispatcher()
                    .Subscribe(value =>
                    {
                        Debug.Assert(value.Sender.StartTime.HasValue);
                        Debug.Assert(value.Sender.Duration.HasValue);

                        if (value.Value)
                        {
                            _checkedStartTimes.TryAdd(value.Sender.StartTime.Value, false);
                            OnAbsenceTimeChange(value.Sender.Duration.Value);
                        }
                        else
                        {
                            _checkedStartTimes.TryRemove(value.Sender.StartTime.Value, out _);
                            OnAbsenceTimeChange(value.Sender.Duration.Value.Negate());
                        }
                    })
                    .DisposeWith(disposables);
            });
        }

        public ReadOnlyObservableCollection<AbsenceRowViewModel> Rows => _rows!;

        private void OnAbsenceTimeChange(TimeSpan e)
        {
            AbsenceTimeChange?.Invoke(this, e);
        }
    }

    public class AbsenceRowViewModel : ReactiveObject
    {
        public AbsenceRowViewModel()
        {
            _duration = this.WhenAnyValue(x => x.StartTime, x => x.EndTime)
                .Select(x => x.Item2 - x.Item1)
                .ToProperty(this, x => x.Duration);

            _canCheck = this.WhenAnyValue(x => x.StartTime, x => x.EndTime)
                .Select(x => x.Item1 != null && x.Item2 != null)
                .ToProperty(this, x => x.CanCheck);
        }
        
        private readonly DateTimeOffset? _startTime;
        public DateTimeOffset? StartTime
        { 
            get => _startTime; 
            init => this.RaiseAndSetIfChanged(ref _startTime, value);
        }

        private readonly DateTimeOffset? _endTime;
        public DateTimeOffset? EndTime
        {
            get => _endTime;
            init => this.RaiseAndSetIfChanged(ref _endTime, value);
        }

        private readonly ObservableAsPropertyHelper<TimeSpan?> _duration;
        public TimeSpan? Duration => _duration.Value;

        private readonly ObservableAsPropertyHelper<bool> _canCheck;
        public bool CanCheck => _canCheck.Value;

        
        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set => this.RaiseAndSetIfChanged(ref _checked, value);
        }
    }
}
