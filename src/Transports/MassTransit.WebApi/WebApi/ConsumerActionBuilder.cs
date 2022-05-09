namespace MassTransit.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;


    public class ConsumerActionBuilder<TConsumer> :
        IConsumerActionBuilder
        where TConsumer : class, IConsumer
    {
        readonly IEndpointNameFormatter _endpointNameFormatter;
        readonly IReadOnlyList<IConsumerMessageActionBuilder> _messageActionBuilders;

        public ConsumerActionBuilder(IEndpointNameFormatter endpointNameFormatter)
        {
            _endpointNameFormatter = endpointNameFormatter;
            _messageActionBuilders = ConsumerMetadataCache<TConsumer>.ConsumerTypes.Select(x => CreateBuilder(x)).ToList();
        }

        public Type ConsumerType => typeof(TConsumer);

        public void Build(ControllerModel controller)
        {
            // use ConsumerDefinition to get endpoint name
            controller.ControllerName = _endpointNameFormatter.Consumer<TConsumer>();

            foreach (var actionBuilder in _messageActionBuilders)
                actionBuilder.Build(controller);
        }

        IConsumerMessageActionBuilder CreateBuilder(IMessageInterfaceType messageInterfaceType)
        {
            var builderType = typeof(ConsumerMessageActionBuilder<,>).MakeGenericType(typeof(TConsumer), messageInterfaceType.MessageType);

            return Activator.CreateInstance(builderType, _endpointNameFormatter) as IConsumerMessageActionBuilder
                ?? throw new InvalidOperationException("Failed to create Consumer Message Action Builder");
        }
    }
}
