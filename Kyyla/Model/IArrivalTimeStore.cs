using System;
using System.Threading.Tasks;

namespace Kyyla.Model
{
    interface IArrivalTimeStore
    {
        Task<DateTimeOffset> GetArrivalTimeAsync();

        Task SetArrivalTimeAsync(DateTimeOffset arrivalTime);

        event EventHandler<DateTimeOffset> ArrivalTimeChanged;
    }
}
