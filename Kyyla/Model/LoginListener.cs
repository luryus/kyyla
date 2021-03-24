using System;
using Microsoft.Win32;

namespace Kyyla.Model
{
    internal class LoginListener : ILoginListener
    {
        public LoginListener()
        {
            SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
        }

        private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionUnlock || e.Reason == SessionSwitchReason.SessionLogon)
            {
                LoginDetected?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            SystemEvents.SessionSwitch -= SystemEventsOnSessionSwitch;
        }

        public event EventHandler? LoginDetected;
    }
}