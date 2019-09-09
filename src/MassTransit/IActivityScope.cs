namespace MassTransit
{
    using System;
    using System.Diagnostics;


    public interface IActivityScope : IDisposable
    {
        Activity Current { get; }
    }


    class ActivityScope : IActivityScope
    {
        readonly DiagnosticSource _source;
        readonly string _name;

        //TODO: Add BeginScope for ILogger
        public ActivityScope(DiagnosticSource source, string name, object args = default)
        {
            _source = source;
            _name = name;
            Current = new Activity(_name);
            Start(args);
        }

        void Stop(object args = default)
        {
            if (IsEnabled(_name))
                _source?.StopActivity(Current, args ?? new { });
        }

        void Start(object args = default)
        {
            if (IsEnabled(_name))
                _source?.StartActivity(Current, args ?? new { });
        }

        public void Dispose()
        {
            Stop();
        }

        public Activity Current { get; }

        bool IsEnabled(string name) => _source.IsEnabled(name);
    }
}
