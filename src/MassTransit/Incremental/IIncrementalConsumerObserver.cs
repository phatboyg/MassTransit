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


    public interface IIncrementalConsumerObserver<TContext, in TMessage>
        where TMessage : class
        where TContext : class
    {
        Task PreInitial(InitialContext<TContext, TMessage> context);

        Task PostInitial(InitialContext<TContext, TMessage> context);

        Task PreIncrement(IncrementContext<TContext, TMessage> context);

        Task PostIncrement(IncrementContext<TContext, TMessage> context);

        Task PreFinal(FinalContext<TContext, TMessage> context);

        Task PostFinal(FinalContext<TContext, TMessage> context);

        Task InitialFault(InitialContext<TContext, TMessage> context, Exception exception);

        Task IncrementFault(IncrementContext<TContext, TMessage> context, Exception exception);

        Task FinalFault(FinalContext<TContext, TMessage> context, Exception exception);
    }
}