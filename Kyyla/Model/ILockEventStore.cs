using System.Threading.Tasks;

namespace Kyyla.Model
{
    public interface ILockEventStore
    {
        Task StoreLockEvent(LockEvent ev);
    }
}
