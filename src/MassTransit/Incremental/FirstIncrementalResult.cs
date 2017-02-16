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


    public class FirstIncrementalResult<TContext, TMessage> :
        BaseIncrementalResult<TContext>,
        IncrementalResult<TContext>
        where TContext : class
        where TMessage : class
    {
        readonly Uri _destinationAddress;
        readonly InitialContext<TContext, TMessage> _initialContext;

        public FirstIncrementalResult(InitialContext<TContext, TMessage> initialContext, TContext context)
            : base(context)
        {
            _initialContext = initialContext;

            _destinationAddress = initialContext.ReceiveContext.InputAddress;
        }

        public Task Send()
        {
            Increment<TContext, TMessage> increment = new Next(_initialContext, this);

            return _initialContext.Send(_destinationAddress, increment);
        }


        class Next :
            Increment<TContext, TMessage>
        {
            readonly IncrementalResult<TContext> _context;
            readonly InitialContext<TContext, TMessage> _initialContext;

            public Next(InitialContext<TContext, TMessage> initialContext, IncrementalResult<TContext> context)
            {
                _initialContext = initialContext;
                _context = context;
            }

            public TMessage Message => _initialContext.Message;
            public TContext Context => _context.Context;
            public Guid IncrementalId => _initialContext.IncrementalId;
            public DateTime StartTimestamp => _initialContext.StartTimestamp;
            public TimeSpan Duration => _initialContext.Duration;
            public long Offset => _context.Offset;
            public long? Length => _context.Length;
            public long? TotalLength => _context.TotalLength;
            public long Index => _context.Index;
            public long? Count => _context.Count;
        }
    }
}