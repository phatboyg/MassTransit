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
    using Context;


    public class HostConsumerInitialContext<TConsumer, TContext, TMessage> :
        ConsumeContextProxy<TMessage>,
        ConsumerInitialContext<TConsumer, TContext, TMessage>
        where TConsumer : class, IIncrementalConsumer
        where TMessage : class
        where TContext : class
    {
        readonly InitialContext<TContext, TMessage> _context;

        public HostConsumerInitialContext(InitialContext<TContext, TMessage> context, TConsumer consumer)
            : base(context)
        {
            _context = context;
            Consumer = consumer;
        }

        IncrementalResult<TContext> InitialContext<TContext, TMessage>.Start(TContext context, long? offset, long? length, long? totalLength, long? index,
            long? count)
        {
            return _context.Start(context, offset, length, totalLength, index, count);
        }

        IncrementalResult<TContext> InitialContext<TContext, TMessage>.Fault(Exception exception)
        {
            return _context.Fault(exception);
        }

        Guid InitialContext.IncrementalId => _context.IncrementalId;
        DateTime InitialContext.StartTimestamp => _context.StartTimestamp;
        TimeSpan InitialContext.ElapsedTime => _context.ElapsedTime;
        TimeSpan InitialContext.Duration => _context.Duration;

        public TConsumer Consumer { get; }
    }
}