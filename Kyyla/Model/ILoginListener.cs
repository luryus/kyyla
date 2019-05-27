using System;


namespace Kyyla.Model
{
    public interface ILoginListener : IDisposable
    {
        event EventHandler LoginDetected;
    }
}
