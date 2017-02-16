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
namespace MassTransit.Incremental.Contracts
{
    using System;


    /// <summary>
    /// Message contract for an increment that is sent back to the consumer via the transport,
    /// which allows increments to be scheduled and support a checkpoint via being written out 
    /// to the durable queue, gives a sense of reliability.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public interface Increment<out TContext, out TMessage>
        where TMessage : class
        where TContext : class
    {
        /// <summary>
        /// The split message
        /// </summary>
        TMessage Message { get; }

        /// <summary>
        /// The incremental context provided by the consumer
        /// </summary>
        TContext Context { get; }

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
        /// Consumer time actually spent processing the incremental message
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// an arbitrary number used for starting the consumer at the next item
        /// </summary>
        long Offset { get; }

        /// <summary>
        /// The length of the increment that should be processed by the consumer, if known
        /// </summary>
        long? Length { get; }

        /// <summary>
        /// the length of the file, content, etc, if known
        /// </summary>
        long? TotalLength { get; }

        /// <summary>
        /// This is the nth time Increment has been called (0-based, of course)
        /// </summary>
        long Index { get; }

        /// <summary>
        /// If known the total number of increments that will be processed
        /// </summary>
        long? Count { get; }
    }
}