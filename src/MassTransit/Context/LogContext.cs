namespace MassTransit.Context
{
    using System;
    using System.Diagnostics;
    using Logging;
    using Microsoft.Extensions.Logging;


    public partial class LogContext
    {
        public static void ConfigureCurrentLogContext(ILoggerFactory loggerFactory = null, DiagnosticSource source = null)
        {
            Current = new BusLogContext(loggerFactory ?? NullLoggerFactory.Instance, source ?? Cached.Default.Value);
        }

        /// <summary>
        /// Configure the current <see cref="LogContext"/> using the specified <paramref name="logger"/>, which will be
        /// used for all log output.
        /// </summary>
        /// <param name="logger">An existing logger</param>
        /// <param name="source">An optional custom <see cref="DiagnosticSource"/></param>
        public static void ConfigureCurrentLogContext(Microsoft.Extensions.Logging.ILogger logger, DiagnosticSource source = null)
        {
            Current = new BusLogContext(new SingleLoggerFactory(logger), source ?? Cached.Default.Value);
        }

        public static void SetCurrentIfNull(ILogContext context)
        {
            if (Current == null)
                Current = context;
        }

        static bool ValidateSetCurrent(ILogContext logContext)
        {
            return true;
        }


        static class Cached
        {
            internal static readonly Lazy<DiagnosticListener> Default =
                new Lazy<DiagnosticListener>(() => new DiagnosticListener(DiagnosticHeaders.DefaultListenerName));
        }


        class NoopActivityScope : IActivityScope
        {
            readonly string _operationName;
            readonly Activity _current;

            public NoopActivityScope(string operationName)
            {
                _operationName = operationName;
                _current = new Activity(_operationName);
            }

            public void Dispose()
            {
            }

            Activity IActivityScope.Current => _current;
        }

        public static IActivityScope StartActivity(string name, object args = null) => Current?.BeginActivity(name, args) ?? new NoopActivityScope(name);

        public static void LogDebug(string message, params object[] args)
        {
            Current?.LogDebug(message, args);
        }

        public static void LogDebug(Exception exception, string message, params object[] args)
        {
            Current?.LogDebug(exception, message, args);
        }

        public static void LogInformation(string message, params object[] args)
        {
            Current?.LogInformation(message, args);
        }

        public static void LogInformation(Exception exception, string message, params object[] args)
        {
            Current?.LogInformation(exception, message, args);
        }

        public static void LogWarning(string message, params object[] args)
        {
            Current?.LogWarning(message, args);
        }

        public static void LogWarning(Exception exception, string message, params object[] args)
        {
            Current?.LogWarning(exception, message, args);
        }

        public static void LogError(string message, params object[] args)
        {
            Current?.LogError(message, args);
        }

        public static void LogError(Exception exception, string message, params object[] args)
        {
            Current?.LogError(exception, message, args);
        }
    }
}
