using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using Serilog;

namespace Kyyla.Model
{
    public class LockEventStore : ILockEventStore, IDisposable
    {
        private readonly ILogger _logger;

        public LockEventStore()
        {
            _logger = Log.ForContext<LockEventStore>();

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        public void Dispose()
        {
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            LockEvent ev;
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    ev = new LockEvent { EventType = LockEvent.Type.Unlock, Timestamp = DateTimeOffset.UtcNow };
                    break;
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                    ev = new LockEvent { EventType = LockEvent.Type.Lock, Timestamp = DateTimeOffset.UtcNow };
                    break;
                default:
                    return;
            }

            Task.Run(async () =>
            {
                await StoreLockEvent(ev);
            });
        }


        public async Task StoreLockEvent(LockEvent ev)
        {
            try
            {
                using var db = new LockEventDbContext();
                await db.AddAsync(ev);
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error storing lock event");
            }
        }
    }
}
