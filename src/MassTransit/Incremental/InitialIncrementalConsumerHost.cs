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
    using System.Diagnostics;
    using System.Threading.Tasks;
    using GreenPipes;
    using Logging;
    using Util;


    public class InitialIncrementalConsumerHost<TConsumer, TContext, TMessage> :
        IFilter<ConsumeContext<TMessage>>
        where TConsumer : class, IIncrementalConsumer<TContext, TMessage>
        where TMessage : class
        where TContext : class
    {
        static readonly ILog _log = Logger.Get<InitialIncrementalConsumerHost<TConsumer, TContext, TMessage>>();

        readonly IIncrementalConsumerFactory<TConsumer, TContext, TMessage> _consumerFactory;

        readonly IRequestPipe<InitialContext<TContext, TMessage>, IncrementalResult<TContext>> _initialPipe;

        public InitialIncrementalConsumerHost(IIncrementalConsumerFactory<TConsumer, TContext, TMessage> consumerFactory, IPipe<RequestContext> executePipe)
        {
            if (consumerFactory == null)
                throw new ArgumentNullException(nameof(consumerFactory));

            _consumerFactory = consumerFactory;

            _initialPipe = executePipe.CreateRequestPipe<InitialContext<TContext, TMessage>, IncrementalResult<TContext>>();
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("initialIncrementalConsumerHost");
            scope.Set(new
            {
                ConsumerType = TypeMetadataCache<TConsumer>.ShortName,
                MessageType = TypeMetadataCache<TMessage>.ShortName
            });

            _initialPipe.Probe(scope);
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                InitialContext<TContext, TMessage> initialContext = new HostInitialContext<TContext, TMessage>(context);

                if (_log.IsDebugEnabled)
                {
                    _log.DebugFormat("Host: {0} Consumer: {1} Initial: {2}", context.ReceiveContext.InputAddress, TypeMetadataCache<TConsumer>.ShortName,
                        initialContext.IncrementalId);
                }

                await Task.Yield();

                try
                {
                    IncrementalResult<TContext> result = await _consumerFactory.Initial(initialContext, _initialPipe).Result().ConfigureAwait(false);

                    await result.Send().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    IncrementalResult<TContext> result = initialContext.Fault(ex);

                    await result.Send().ConfigureAwait(false);
                }

                await context.NotifyConsumed(timer.Elapsed, TypeMetadataCache<TConsumer>.ShortName).ConfigureAwait(false);

                await next.Send(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.Error($"The activity {TypeMetadataCache<TConsumer>.ShortName} threw an exception", ex);

                await context.NotifyFaulted(timer.Elapsed, TypeMetadataCache<TConsumer>.ShortName, ex).ConfigureAwait(false);

                throw;
            }
        }
    }
}