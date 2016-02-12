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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Pipeline;
    using Util;

    // Values in Serilog are simplified down into a lowest-common-denominator internal
    // type system so that there is a better chance of code written with one sink in
    // mind working correctly with any other. This technique also makes the programmer
    // writing a log event (roughly) in control of the cost of recording that event.

    /// <summary>
    /// Tracks telemetry, capturing the parent if available, and uses the CallContext to 
    /// propogate the telemetry through the system.
    /// </summary>
    public class TelemetryContext :
        CallContextReference<ITelemetryContext>,
        ITelemetryContext,
        IDisposable
    {
        static readonly MessageTemplateProcessor _templateParser =
            new MessageTemplateProcessor(new PropertyValueConverter(1, Type.EmptyTypes, Enumerable.Empty<IDestructuringPolicy>()));

        readonly Stopwatch _elapsed;
        readonly LogEventSeverity _minSeverity;
        readonly Guid _operationId;

        readonly IPipe<LogEventContext> _output;
        ITelemetryData _data;
        readonly DateTimeOffset _started;

        public TelemetryContext(IPipe<LogEventContext> output, LogEventSeverity minSeverity)
        {
            _minSeverity = minSeverity;
            _output = output;

            _operationId = NewId.NextGuid();
            _started = DateTimeOffset.Now;
            _elapsed = Stopwatch.StartNew();

            Attach();
        }

        protected ITelemetryData Data
        {
            get
            {
                if (_data != null)
                    return _data;

                Interlocked.CompareExchange(ref _data, null, new TelemetryData());

                return _data;
            }
        }

        public static ITelemetryContext Current
        {
            get
            {
                var current = FromCallContext() as ITelemetryContext;
                if (current == null)
                    throw new InvalidOperationException("The current TelemetryContext is not set.");

                return current;
            }
        }

        public static ITelemetryContext CurrentOrDefault
        {
            get
            {
                var current = FromCallContext() as ITelemetryContext;

                var pipe = Pipe.Execute<LogEventContext>(context => context.LogEvent.Template.Render(context.LogEvent.Properties, Console.Out));

                return current ?? new TelemetryContext(pipe, LogEventSeverity.Verbose);
            }
        }

        public void Dispose()
        {
            Detach();
        }

        public Guid OperationId => _operationId;
        public DateTimeOffset Started => _started;
        public TimeSpan Elapsed => _elapsed.Elapsed;

        void ITelemetryContext.Add<T>(string key, T data)
        {
            Data.Add(key, data);
        }

        void ITelemetryContext.Set<T>(string key, T data)
        {
            Data.Set(key, data);
        }

        public void Write(LogEventSeverity severity, string template, params object[] propertyValues)
        {
            Write(severity, null, template, propertyValues);
        }

        public Task Write(LogEventSeverity severity, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            if (messageTemplate == null)
                return TaskUtil.Completed;
            if (!IsEnabled(severity))
                return TaskUtil.Completed;

            // Catch a common pitfall when a single non-object array is cast to object[]
            if (propertyValues != null &&
                propertyValues.GetType() != typeof(object[]))
                propertyValues = new object[] {propertyValues};

            var now = DateTimeOffset.Now;

            MessageTemplate parsedTemplate;
            IEnumerable<TelemetryLogEventProperty> properties;
            _templateParser.Process(messageTemplate, propertyValues, out parsedTemplate, out properties);

            var logEvent = new TelemetryLogEvent(now, severity, parsedTemplate, properties);

            return Dispatch(logEvent);
        }

        public bool IsEnabled(LogEventSeverity severity)
        {
            if ((int)severity < (int)_minSeverity)
                return false;

//            return _levelSwitch == null ||
//                (int)level >= (int)_levelSwitch.MinimumLevel;

            return true;
        }

        public void Write(TelemetryLogEvent logEvent)
        {
            if (logEvent == null)
                return;
            if (!IsEnabled(logEvent.Severity))
                return;

            Dispatch(logEvent);
        }

        public void Verbose(string messageTemplate, params object[] propertyValues)
        {
            Verbose(null, messageTemplate, propertyValues);
        }

        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(LogEventSeverity.Verbose, exception, messageTemplate, propertyValues);
        }

        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            Debug(null, messageTemplate, propertyValues);
        }

        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(LogEventSeverity.Debug, exception, messageTemplate, propertyValues);
        }

        public void Information(string messageTemplate, params object[] propertyValues)
        {
            Information(null, messageTemplate, propertyValues);
        }

        public void Information(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(LogEventSeverity.Information, exception, messageTemplate, propertyValues);
        }

        public void Warning(string messageTemplate, params object[] propertyValues)
        {
            Warning(null, messageTemplate, propertyValues);
        }

        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(LogEventSeverity.Warning, exception, messageTemplate, propertyValues);
        }

        public void Error(string messageTemplate, params object[] propertyValues)
        {
            Error(null, messageTemplate, propertyValues);
        }

        public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(LogEventSeverity.Error, exception, messageTemplate, propertyValues);
        }

        public void Fatal(string messageTemplate, params object[] propertyValues)
        {
            Fatal(null, messageTemplate, propertyValues);
        }

        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Write(LogEventSeverity.Fatal, exception, messageTemplate, propertyValues);
        }

        public ITelemetryContext CreateNestedContext(string propertyName, object propertyValue, bool destructureObjects = false)
        {
            var property = _templateParser.CreateProperty(propertyName, propertyValue, destructureObjects);

            var filter = new PropertyValueFilter(property);

            return new TelemetryContext(_output, _minSeverity);
        }

        Task Dispatch(TelemetryLogEvent logEvent)
        {
            var context = new TelemetryLogEventContext(logEvent);

            return _output.Send(context);
        }
    }
}