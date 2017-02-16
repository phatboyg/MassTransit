namespace MassTransit.PipeConfigurators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConsumeConfigurators;
    using Courier;
    using Courier.Hosts;
    using Courier.Pipeline;
    using GreenPipes;
    using GreenPipes.Builders;
    using Incremental;


    public class IncrementalConsumerHostSpecification<TConsumer, TContext, TMessage> :
        IIncrementalConsumerConfigurator<TConsumer, TContext, TMessage>,
        IReceiveEndpointSpecification
        where TConsumer : class, IIncrementalConsumer<TContext, TMessage>
        where TContext : class
        where TMessage : class
    {
        readonly IIncrementalConsumerFactory<TConsumer, TContext, TMessage> _consumerFactory;
        readonly Func<IPipe<RequestContext>, IFilter<ConsumeContext<TMessage>>> _filterFactory;
        InitialIncrementalConfigurator<TConsumer, TContext, TMessage> _initialSpecification;
        IncrementIncrementalConfigurator<TConsumer, TContext, TMessage> _incrementSpecification;

        public IncrementalConsumerHostSpecification(IIncrementalConsumerFactory<TConsumer, TContext, TMessage> consumerFactory)
        {
            _consumerFactory = consumerFactory;

            _initialSpecification = new InitialIncrementalConfigurator<TConsumer, TContext, TMessage>();
            _incrementSpecification = new IncrementIncrementalConfigurator<TConsumer, TContext, TMessage>();

            _filterFactory = executePipe => new InitialIncrementalConsumerHost<TConsumer,TContext,TMessage>(_consumerFactory, executePipe);
        }


        public void Initial(Action<IInitialIncrementalConfigurator<TConsumer, TContext, TMessage>> configure)
        {
            configure?.Invoke(_initialSpecification);
        }

        public void Increment(Action<IIncrementIncrementalConfigurator<TConsumer, TContext, TMessage>> configure)
        {
            configure?.Invoke(_incrementSpecification);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_filterFactory == null)
                yield return this.Failure("FilterFactory", "must not be null");

            foreach (var result in _initialSpecification.Validate())
                yield return result;

            foreach (var result in _incrementSpecification.Validate())
                yield return result;
        }

        public void Configure(IReceiveEndpointBuilder builder)
        {
            IPipe<RequestContext> executeActivityPipe = _initialSpecification.Build(new ExecuteActivityFilter<TConsumer, TContext>());

            _routingSlipConfigurator.UseFilter(_filterFactory(executeActivityPipe));

            builder.ConnectConsumePipe(_routingSlipConfigurator.Build());
        }
    }
}