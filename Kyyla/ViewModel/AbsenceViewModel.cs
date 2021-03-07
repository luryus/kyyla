using ReactiveUI;
using System;
using DynamicData;

namespace Kyyla.ViewModel
{
    public class AbsenceViewModel : ReactiveObject
    {
        public class RowViewModel : ReactiveObject
        {
            private DateTimeOffset _startTime;
            public DateTimeOffset StartTime
            { 
                get => _startTime; 
                set => this.RaiseAndSetIfChanged(ref _startTime, value);
            }

            private DateTimeOffset _endTime;
            public DateTimeOffset EndTime
            {
                get => _startTime;
                set => this.RaiseAndSetIfChanged(ref _endTime, value);
            }

            public DynamicData.
        }
    }
}
