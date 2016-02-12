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
    using System.Threading.Tasks;
    using Pipeline;


    class PropertyValueFilter :
        IFilter<LogEventContext>
    {
        readonly TelemetryLogEventProperty _property;

        public PropertyValueFilter(TelemetryLogEventProperty property)
        {
            _property = property;
        }

        public Task Send(LogEventContext context, IPipe<LogEventContext> next)
        {
            context.LogEvent.GetOrAddProperty(_property);

            return next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("propertyValue");
            scope.Add("name", _property.Name);
        }
    }
}