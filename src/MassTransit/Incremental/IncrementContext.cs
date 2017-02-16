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


    public interface IncrementContext<TContext, out TMessage> :
        ConsumeContext<TMessage>,
        IncrementContext<TContext>
        where TMessage : class
        where TContext : class
    {
    }


    public interface IncrementContext<TContext> :
        InitialContext,
        IncrementContext
        where TContext : class
    {
        /// <summary>
        /// The context supplied by the initial message consumer
        /// </summary>
        TContext Context { get; }

        /// <summary>
        /// The current increment was processed, and the next increment should be started
        /// </summary>
        /// <param name="context">The context passed to the first increment</param>
        /// <param name="offset">The offset at which the first increment should start processing</param>
        /// <param name="length"></param>
        /// <param name="totalLength"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        IncrementalResult<TContext> Next(long offset, TContext context = null, long? length = default(long?),
            long? totalLength = default(long?), long? index = default(long?), long? count = default(long?));

        /// <summary>
        /// The last increment was processed, and the next step is to call Final
        /// </summary>
        /// <param name="context">The context passed to the first increment</param>
        /// <param name="offset">The offset at which the first increment should start processing</param>
        /// <param name="length"></param>
        /// <param name="totalLength"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        IncrementalResult<TContext> Last(long? offset = default(long?), TContext context = null, long? length = default(long?),
            long? totalLength = default(long?), long? index = default(long?), long? count = default(long?));

        /// <summary>
        /// Fault the context, throwing the exception upon evaluation
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        IncrementalResult<TContext> Fault(Exception exception);

    }


    public interface IncrementContext
    {
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