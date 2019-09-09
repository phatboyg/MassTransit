namespace MassTransit.Context
{
    using System;
    using Microsoft.Extensions.Logging;


    public static class LogContextExtensions
    {
        public static void LogDebug(this ILogContext logContext, string message, params object[] args)
        {
            logContext?.Logger?.LogDebug(message, args);
        }

        public static void LogDebug(this ILogContext logContext, Exception exception, string message, params object[] args)
        {
            logContext?.Logger?.LogDebug(0, exception, message, args);
        }

        public static void LogInformation(this ILogContext logContext, string message, params object[] args)
        {
            logContext?.Logger?.LogInformation(message, args);
        }

        public static void LogInformation(this ILogContext logContext, Exception exception, string message, params object[] args)
        {
            logContext?.Logger?.LogInformation(0, exception, message, args);
        }

        public static void LogWarning(this ILogContext logContext, string message, params object[] args)
        {
            logContext?.Logger?.LogWarning(message, args);
        }

        public static void LogWarning(this ILogContext logContext, Exception exception, string message, params object[] args)
        {
            logContext?.Logger?.LogWarning(0, exception, message, args);
        }

        public static void LogError(this ILogContext logContext, string message, params object[] args)
        {
            logContext?.Logger?.LogError(message, args);
        }

        public static void LogError(this ILogContext logContext, Exception exception, string message, params object[] args)
        {
            logContext?.Logger?.LogError(0, exception, message, args);
        }
    }
}
