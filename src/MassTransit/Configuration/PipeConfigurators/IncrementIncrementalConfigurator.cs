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
namespace MassTransit.PipeConfigurators
{
    using System.Collections.Generic;
    using GreenPipes;
    using GreenPipes.Builders;
    using GreenPipes.Configurators;
    using Incremental;


    public class IncrementIncrementalConfigurator<TConsumer, TContext, TMessage> :
        IBuildPipeConfigurator<ConsumerIncrementContext<TConsumer, TContext, TMessage>>,
        IIncrementIncrementalConfigurator<TConsumer, TContext, TMessage>
        where TConsumer : class, IIncrementalConsumer
        where TContext : class
        where TMessage : class
    {
        readonly IBuildPipeConfigurator<ConsumerIncrementContext<TConsumer, TContext, TMessage>> _configurator;

        public IncrementIncrementalConfigurator()
        {
            _configurator = new PipeConfigurator<ConsumerIncrementContext<TConsumer, TContext, TMessage>>();
        }

        public IPipe<ConsumerIncrementContext<TConsumer, TContext, TMessage>> Build()
        {
            return _configurator.Build();
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return _configurator.Validate();
        }

        public void AddPipeSpecification(IPipeSpecification<ConsumerIncrementContext<TConsumer, TContext, TMessage>> specification)
        {
            _configurator.AddPipeSpecification(specification);
        }
    }
}