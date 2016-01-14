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
    public interface ITelemetryData
    {
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
    }
}