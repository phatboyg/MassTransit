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
namespace MassTransit.Telemetry.Filters
{
    using System.Threading.Tasks;
    using Pipeline;
    using Util;


    /// <summary>
    /// This is a dumb example of a log event filter. It's dumb because the pipeline
    /// should skip these log events before they even get created, but it's a placeholder
    /// for something better.
    /// </summary>
    public class SeverityLogEventFilter :
        IFilter<LogEventContext>
    {
        readonly LogEventSeverity _minimumSeverity;

        public SeverityLogEventFilter(LogEventSeverity minimumSeverity)
        {
            _minimumSeverity = minimumSeverity;
        }

        public Task Send(LogEventContext context, IPipe<LogEventContext> next)
        {
            if (context.LogEvent.Severity < _minimumSeverity)
                return TaskUtil.Completed;

            return next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("severity");
            scope.Add("minimumSeverity", _minimumSeverity.ToString());
        }
    }
}