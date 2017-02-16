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
    using System.Linq;
    using GreenPipes;
    using GreenPipes.Builders;
    using GreenPipes.Configurators;
    using GreenPipes.Contexts;
    using Incremental;


    public class InitialIncrementalConfigurator<TConsumer, TContext, TMessage> :
        IInitialIncrementalConfigurator<TConsumer, TContext, TMessage>
        where TConsumer : class, IIncrementalConsumer
        where TContext : class
        where TMessage : class
    {
        readonly IBuildPipeConfigurator<ConsumerInitialContext<TConsumer, TContext, TMessage>> _configurator;
        readonly IList<IPipeSpecification<ConsumerInitialContext<TConsumer, TContext, TMessage>>> _specifications;

        public InitialIncrementalConfigurator()
        {
            _configurator = new PipeConfigurator<ConsumerInitialContext<TConsumer, TContext, TMessage>>();

            _specifications = new List<IPipeSpecification<ConsumerInitialContext<TConsumer, TContext, TMessage>>>();
        }

        public IPipe<RequestContext> Build()
        {
            return Pipe.New<RequestContext>(cfg =>
            {
                cfg.UseDispatch(new RequestConverterFactory(), d =>
                {
                    d.Pipe<RequestContext<InitialContext<TContext, TMessage>, IncrementalResult<TContext>>(h =>
                    {

                        AddFilters(builders, h);

                        h.UseFilter(consumeFilter);
                    });

                    d.Handle<InitialContext<TContext, TMessage>>();
                });
            });
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return _specifications.SelectMany(x => x.Validate());
        }

        public void AddPipeSpecification(IPipeSpecification<ConsumerInitialContext<TConsumer, TContext, TMessage>> specification)
        {
            _specifications.Add(specification);
        }
    }
}