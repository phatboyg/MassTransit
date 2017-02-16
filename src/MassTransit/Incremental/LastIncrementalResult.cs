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
    using Contracts;


    public class LastIncrementalResult<TContext, TMessage> :
        BaseIncrementalResult<TContext>,
        IncrementalResult<TContext>
        where TContext : class
        where TMessage : class
    {
        readonly Uri _destinationAddress;
        readonly IncrementContext<TContext, TMessage> _incrementContext;

        public LastIncrementalResult(IncrementContext<TContext, TMessage> incrementContext, TContext context)
            : base(context)
        {
            _incrementContext = incrementContext;

            _destinationAddress = incrementContext.ReceiveContext.InputAddress;
        }

        public Task Send()
        {
            Final<TContext, TMessage> increment = new Last(_incrementContext, this);

            return _incrementContext.Send(_destinationAddress, increment);
        }


        class Last :
            Final<TContext, TMessage>
        {
            readonly IncrementalResult<TContext> _context;
            readonly IncrementContext<TContext, TMessage> _incrementContext;

            public Last(IncrementContext<TContext, TMessage> incrementContext, IncrementalResult<TContext> context)
            {
                _incrementContext = incrementContext;
                _context = context;
            }

            public TMessage Message => _incrementContext.Message;
            public TContext Context => _context.Context;
            public Guid IncrementalId => _incrementContext.IncrementalId;
            public DateTime StartTimestamp => _incrementContext.StartTimestamp;
            public TimeSpan Duration => _incrementContext.Duration;
            public long Offset => _context.Offset;
            public long? Length => _context.Length;
            public long? TotalLength => _context.TotalLength;
            public long Index => _context.Index;
            public long? Count => _context.Count;
        }
    }
}