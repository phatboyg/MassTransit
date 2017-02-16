// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Incremental
{
    using System;


    public interface InitialContext<TContext, out TMessage> :
        ConsumeContext<TMessage>,
        InitialContext
        where TContext : class
        where TMessage : class
    {
        /// <summary>
        /// Starts the incremental consumption of the initial message
        /// </summary>
        /// <param name="context">The context passed to the first increment</param>
        /// <param name="offset">The offset at which the first increment should start processing</param>
        /// <param name="length"></param>
        /// <param name="totalLength"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        IncrementalResult<TContext> Start(TContext context, long? offset = default(long?), long? length = default(long?),
            long? totalLength = default(long?), long? index = default(long?), long? count = default(long?));

        /// <summary>
        /// Fault the context, throwing the exception upon evaluation
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        IncrementalResult<TContext> Fault(Exception exception);
    }


    public interface InitialContext
    {
        /// <summary>
        /// Generated upon receive of the initial message, used to keep everything together
        /// and can be used as a partition key to keep processing on the same node, etc.
        /// </summary>
        Guid IncrementalId { get; }

        /// <summary>
        /// When the initial message was received, starts the elapsed timer
        /// </summary>
        DateTime StartTimestamp { get; }

        /// <summary>
        /// Elapsed time since the initial message was received
        /// </summary>
        TimeSpan ElapsedTime { get; }

        /// <summary>
        /// Consumer time actually spent processing the incremental message
        /// </summary>
        TimeSpan Duration { get; }
    }
}