using System;
using System.Threading.Tasks;
using DynamicData;

namespace Kyyla.Model
{
    public interface ILockEventStore
    {
        Task StoreLockEvent(LockEvent ev);
        IObservableList<(DateTimeOffset?, DateTimeOffset?)> CurrentDayLockEvents { get; }
    }
}
