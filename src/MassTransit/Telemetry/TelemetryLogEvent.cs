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
    /// A log event emitted by a telemetry context
    /// </summary>
    public class TelemetryLogEvent
    {
        public TelemetryLogEvent(DateTimeOffset timestamp, LogEventSeverity severity, MessageTemplate template,
            IEnumerable<TelemetryLogEventProperty> properties)
        {
            Timestamp = timestamp;
            Severity = severity;
            Template = template;
            Properties = properties.Distinct().ToDictionary(x => x.Name, x => x.Value);
        }

        public DateTimeOffset Timestamp { get; set; }
        public LogEventSeverity Severity { get; }
        public MessageTemplate Template { get; set; }
        public IReadOnlyDictionary<string, TelemetryLogEventPropertyValue> Properties { get; set; }
    }
}