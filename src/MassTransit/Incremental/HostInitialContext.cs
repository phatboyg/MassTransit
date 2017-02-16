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


    public class HostInitialContext<TContext, TMessage> :
        ConsumeContextProxy<TMessage>,
        InitialContext<TContext, TMessage>
        where TContext : class
        where TMessage : class
    {
        readonly ConsumeContext<TMessage> _context;

        public HostInitialContext(ConsumeContext<TMessage> context)
            : base(context)
        {
            _context = context;

            IncrementalId = context.MessageId ?? NewId.NextGuid();
        }

        public Guid IncrementalId { get; }
        public DateTime StartTimestamp => DateTime.UtcNow - _context.ReceiveContext.ElapsedTime;
        public TimeSpan ElapsedTime => _context.ReceiveContext.ElapsedTime;
        public TimeSpan Duration => _context.ReceiveContext.ElapsedTime;

        IncrementalResult<TContext> InitialContext<TContext, TMessage>.Start(TContext context, long? offset, long? length, long? totalLength, long? index,
            long? count)
        {
            return new FirstIncrementalResult<TContext, TMessage>(this, context)
            {
                Offset = offset ?? 0L,
                Length = length,
                TotalLength = totalLength,
                Index = index ?? 0L,
                Count = count
            };
        }

        IncrementalResult<TContext> InitialContext<TContext, TMessage>.Fault(Exception exception)
        {
            return new FaultIncrementalResult<TContext>(exception);
        }
    }
}