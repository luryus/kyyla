﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Serilog;

namespace Kyyla.Model
{
    public class LockEventStore : ILockEventStore, IDisposable
    {
        private readonly ILogger _logger;
        private readonly SourceList<(DateTimeOffset?, DateTimeOffset?)> _lockEventPairList;

        public LockEventStore()
        {
            _logger = Log.ForContext<LockEventStore>();
            _lockEventPairList = new SourceList<(DateTimeOffset?, DateTimeOffset?)>();

            Task.Run(async () =>
            {
                await using var db = new LockEventDbContext();
                await ReloadCurrentDayEvents(db);
            });
            
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            //_lockEventPairList.Connect().Subscribe(x => Debugger.Break());
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
                case SessionSwitchReason.SessionUnlock:
                    ev = new LockEvent { EventType = LockEvent.Type.Unlock, Timestamp = DateTime.Now };
                    break;
                case SessionSwitchReason.SessionLock:
                    ev = new LockEvent { EventType = LockEvent.Type.Lock, Timestamp = DateTime.Now };
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
                await using var db = new LockEventDbContext();
                await db.AddAsync(ev);
                await db.SaveChangesAsync();
                await ReloadCurrentDayEvents(db);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error storing lock event");
            }
        }

        private async Task ReloadCurrentDayEvents(LockEventDbContext db)
        {
            try
            {
                var today = DateTime.Now.Date;
                var tomorrow = today.AddDays(1);
                var events = await db.LockEvents
                    .Where(e => e.Timestamp >= today && e.Timestamp < tomorrow)
                    .OrderBy(e => e.Timestamp)
                    .ToListAsync();
                
                // Make pairs out of the event times
                var pairs = new List<(DateTimeOffset?, DateTimeOffset?)>(events.Count);
                DateTimeOffset? pairStartTime = null;
                foreach (var t in events)
                {
                    if (t.EventType == LockEvent.Type.Lock)
                    {
                        if (pairStartTime != null)
                        {
                            pairs.Add((pairStartTime, null));
                        }

                        pairStartTime = t.Timestamp;
                    }
                    else
                    {
                        pairs.Add((pairStartTime, t.Timestamp));
                        pairStartTime = null;
                    }
                }

                if (pairStartTime != null)
                {
                    pairs.Add((pairStartTime, null));
                }

                _lockEventPairList.EditDiff(pairs);
                
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error reloading lock events from db");
            }
        }
        
        public IObservableList<(DateTimeOffset?, DateTimeOffset?)> CurrentDayLockEvents => _lockEventPairList.AsObservableList();
    }

}
