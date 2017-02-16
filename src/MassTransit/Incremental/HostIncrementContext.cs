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
    using System.Threading.Tasks;
    using Context;
    using Contracts;


    public class HostIncrementContext<TContext, TMessage> :
        ConsumeContextProxy,
        IncrementContext<TContext, TMessage>
        where TMessage : class
        where TContext : class
    {
        readonly ConsumeContext<Increment<TContext, TMessage>> _context;

        public HostIncrementContext(ConsumeContext<Increment<TContext, TMessage>> context)
            : base(context)
        {
            _context = context;
        }

        public Guid IncrementalId => _context.Message.IncrementalId;
        public DateTime StartTimestamp => _context.Message.StartTimestamp;
        public TimeSpan ElapsedTime => DateTime.UtcNow - _context.Message.StartTimestamp;
        public TMessage Message => _context.Message.Message;
        public TimeSpan Duration => _context.Message.Duration + _context.ReceiveContext.ElapsedTime;
        public long Offset => _context.Message.Offset;
        public long? Length => _context.Message.Length;
        public long? TotalLength => _context.Message.TotalLength;
        public long Index => _context.Message.Index;
        public long? Count => _context.Message.Count;
        public TContext Context => _context.Message.Context;

        public virtual Task NotifyConsumed(TimeSpan duration, string consumerType)
        {
            return base.NotifyConsumed(this, duration, consumerType);
        }

        public virtual Task NotifyFaulted(TimeSpan duration, string consumerType, Exception exception)
        {
            return base.NotifyFaulted(this, duration, consumerType, exception);
        }

        IncrementalResult<TContext> IncrementContext<TContext>.Next(long offset, TContext context, long? length, long? totalLength, long? index,
            long? count)
        {
            if (offset <= _context.Message.Offset)
                throw new ArgumentOutOfRangeException(nameof(offset),
                    $"The next offset must be greater than the current offset: {offset} <= {_context.Message.Offset}");
            if (index.HasValue && index.Value <= _context.Message.Index)
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"The next offset must be greater than the current offset: {index.Value} <= {_context.Message.Index}");

            return new NextIncrementalResult<TContext, TMessage>(this, context)
            {
                Offset = offset,
                Length = length ?? _context.Message.Length,
                TotalLength = totalLength ?? _context.Message.TotalLength,
                Index = index ?? _context.Message.Index + 1,
                Count = count ?? _context.Message.Count
            };
        }

        IncrementalResult<TContext> IncrementContext<TContext>.Last(long? offset, TContext context, long? length, long? totalLength, long? index,
            long? count)
        {
            if (offset < _context.Message.Offset)
                throw new ArgumentOutOfRangeException(nameof(offset),
                    $"The next offset must be greater than the current offset: {offset} <= {_context.Message.Offset}");
            if (index.HasValue && index.Value < _context.Message.Index)
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"The next offset must be greater than the current offset: {index.Value} <= {_context.Message.Index}");

            return new LastIncrementalResult<TContext, TMessage>(this, context)
            {
                Offset = offset ?? _context.Message.Offset,
                Length = length ?? _context.Message.Length,
                TotalLength = totalLength ?? _context.Message.TotalLength,
                Index = index ?? _context.Message.Index + 1,
                Count = count ?? _context.Message.Count
            };
        }

        IncrementalResult<TContext> IncrementContext<TContext>.Fault(Exception exception)
        {
            return new FaultIncrementalResult<TContext>(exception);
        }
    }
}