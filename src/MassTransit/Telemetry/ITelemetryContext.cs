// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Telemetry
{
    using System;
    using System.Threading.Tasks;


    /// <summary>
    /// Telemetry is a new way of logging that ensures continuity during message consumption and publication.
    /// </summary>
    public interface ITelemetryContext
    {
        /// <summary>
        /// The unique identifier for this operation
        /// </summary>
        Guid OperationId { get; }

        /// <summary>
        /// When the operation was started
        /// </summary>
        DateTimeOffset Started { get; }

        /// <summary>
        /// Elapsed time for the operation, thus far
        /// </summary>
        TimeSpan Elapsed { get; }

        /// <summary>
        /// Add telemetry data that can be used in telemetry output. Telemetry data is promoted from an element to
        /// an element list if the same key is added multiple times.
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="key">The key for the data</param>
        /// <param name="data">The data</param>
        void Add<T>(string key, T data)
            where T : class;

        /// <summary>
        /// Set the telemetry data that can be used in telemetry output. 
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="key">The key for the data</param>
        /// <param name="data">The data</param>
        void Set<T>(string key, T data)
            where T : class;

        /// <summary>
        /// Write an event to the log.
        /// </summary>
        /// <param name="logEvent">The event to write.</param>
        void Write(TelemetryLogEvent logEvent);

        /// <summary>
        /// Write a log event with the specified level.
        /// </summary>
        /// <param name="severity">The level of the event.</param>
        /// <param name="template"></param>
        /// <param name="propertyValues"></param>
        void Write(LogEventSeverity severity, string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the specified level and associated exception.
        /// </summary>
        /// <param name="severity">The level of the event.</param>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        Task Write(LogEventSeverity severity, Exception exception, string template, params object[] propertyValues);

        /// <summary>
        /// Determine if events at the specified level will be passed through
        /// to the log sinks.
        /// </summary>
        /// <param name="severity">Level to check.</param>
        /// <returns>True if the level is enabled; otherwise, false.</returns>
        bool IsEnabled(LogEventSeverity severity);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Verbose"/> level and associated exception.
        /// </summary>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Verbose("Staring into space, wondering if we're alone.");
        /// </example>
        void Verbose(string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Verbose"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
        /// </example>
        void Verbose(Exception exception, string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Debug"/> level and associated exception.
        /// </summary>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
        /// </example>
        void Debug(string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Debug"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Debug(ex, "Swallowing a mundane exception.");
        /// </example>
        void Debug(Exception exception, string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Information"/> level and associated exception.
        /// </summary>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        void Information(string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Information"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
        /// </example>
        void Information(Exception exception, string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Warning"/> level and associated exception.
        /// </summary>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        void Warning(string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Warning"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
        /// </example>
        void Warning(Exception exception, string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Error"/> level and associated exception.
        /// </summary>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        void Error(string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Error"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
        /// </example>
        void Error(Exception exception, string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Fatal"/> level and associated exception.
        /// </summary>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Fatal("Process terminating.");
        /// </example>
        void Fatal(string template, params object[] propertyValues);

        /// <summary>
        /// Write a log event with the <see cref="LogEventSeverity.Fatal"/> level and associated exception.
        /// </summary>
        /// <param name="exception">Exception related to the event.</param>
        /// <param name="template">Message template describing the event.</param>
        /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
        /// <example>
        /// Log.Fatal(ex, "Process terminating.");
        /// </example>
        void Fatal(Exception exception, string template, params object[] propertyValues);
    }
}