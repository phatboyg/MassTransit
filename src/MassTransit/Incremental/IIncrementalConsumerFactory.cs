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
    using System.Threading.Tasks;
    using GreenPipes;


    public interface IIncrementalConsumerFactory<TConsumer, TContext, TMessage>
        where TConsumer : class, IIncrementalConsumer<TContext, TMessage>
        where TMessage : class
        where TContext : class
    {
        Task<ResultContext<IncrementalResult<TContext>>> Initial(InitialContext<TContext, TMessage> context,
            IRequestPipe<ConsumerInitialContext<TConsumer, TContext, TMessage>, IncrementalResult<TContext>> next);

        Task<ResultContext<IncrementalResult<TContext>>> Increment(IncrementContext<TContext, TMessage> context,
            IRequestPipe<ConsumerIncrementContext<TConsumer, TContext, TMessage>, IncrementalResult<TContext>> next);

        Task Final(ConsumerIncrementContext<TConsumer, TContext, TMessage> context, IPipe<ConsumerIncrementContext<TConsumer, TContext, TMessage>> next);
    }
}