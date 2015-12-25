// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Diagnostics;

    /// <summary>
    /// Tracks telemetry, capturing the parent if available, and uses the CallContext to 
    /// propogate the telemetry through the system.
    /// </summary>
    public class Telemetry :
        CallContextReference<ITelemetry>,
        ITelemetry,
        IDisposable
    {
        readonly Stopwatch _elapsed;
        NewId _operationNewId;
        Guid _timer;

        public Telemetry()
        {
            _operationNewId = NewId.Next();
            _timer = _operationNewId.ToGuid();
            _elapsed = Stopwatch.StartNew();

            Attach();
        }

        public void Dispose()
        {
            Detach();
        }

        public Guid OperationId => _operationNewId.ToGuid();
        public DateTime Started => _operationNewId.Timestamp;
        public TimeSpan Elapsed => _elapsed.Elapsed;
    }
}