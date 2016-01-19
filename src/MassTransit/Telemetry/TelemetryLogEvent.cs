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
    using System.Linq;


    /// <summary>
    /// A log event
    /// </summary>
    public class TelemetryLogEvent
    {
        readonly Dictionary<string, TelemetryLogEventPropertyValue> _propertyValues;

        public TelemetryLogEvent(DateTimeOffset timestamp, LogEventSeverity severity, MessageTemplate template,
            IEnumerable<TelemetryLogEventProperty> properties)
        {
            Timestamp = timestamp;
            Severity = severity;
            Template = template;

            _propertyValues = properties.Distinct().ToDictionary(x => x.Name, x => x.Value);
        }

        /// <summary>
        /// The time when the event occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The severity of the event
        /// </summary>
        public LogEventSeverity Severity { get; }

        /// <summary>
        /// The message template supplied (unformatted)
        /// </summary>
        public MessageTemplate Template { get; }

        /// <summary>
        /// The properties for the log event
        /// </summary>
        public IReadOnlyDictionary<string, TelemetryLogEventPropertyValue> Properties => _propertyValues;

        public void GetOrAddProperty(TelemetryLogEventProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (!_propertyValues.ContainsKey(property.Name))
                _propertyValues.Add(property.Name, property.Value);
        }
    }
}