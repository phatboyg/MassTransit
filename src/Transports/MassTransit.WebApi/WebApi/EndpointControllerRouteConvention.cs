namespace MassTransit.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using DependencyInjection.Registration;
    using Internals;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.Extensions.DependencyInjection;


    /// <summary>
    /// Endpoints have a name, which is used to prefix the route for the methods in the controller.
    /// </summary>
    public class EndpointControllerRouteConvention :
        IControllerModelConvention
    {
        readonly IReadOnlyDictionary<Type, IConsumerActionBuilder> _consumerActionBuilders;
        readonly IReadOnlyDictionary<Type, ISagaActionBuilder> _sagaActionBuilders;

        public EndpointControllerRouteConvention(IEnumerable<IConsumerRegistration> consumers, IEnumerable<ISagaRegistration> sagas,
            IEndpointNameFormatter formatter, IServiceProvider provider)
        {
            _consumerActionBuilders = consumers
                .Select(type => Activator.CreateInstance(typeof(ConsumerActionBuilder<>).MakeGenericType(type.Type), formatter) as IConsumerActionBuilder
                    ?? throw new InvalidOperationException("Failed to create Consumer Action Builder"))
                .ToDictionary(x => x.ConsumerType);

            var sagaActionBuilders = new Dictionary<Type, ISagaActionBuilder>();
            foreach (var registration in sagas)
            {
                if (registration.GetType().ClosesType(typeof(SagaStateMachineRegistration<,>), out Type[] _))
                {
                    var stateMachine = provider.GetRequiredService(typeof(SagaStateMachine<>).MakeGenericType(registration.Type));

                    var builderType = typeof(SagaStateMachineActionBuilder<>).MakeGenericType(registration.Type);
                    var builder = Activator.CreateInstance(builderType, stateMachine, formatter) as ISagaActionBuilder
                        ?? throw new InvalidOperationException("Failed to create Saga Action Builder");

                    sagaActionBuilders.Add(registration.Type, builder);
                }
            }

            _sagaActionBuilders = sagaActionBuilders;
        }

        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.ClosesType(typeof(ConsumerEndpointController<>), out Type[] consumerTypes))
            {
                if (_consumerActionBuilders.TryGetValue(consumerTypes[0], out var builder))
                    builder.Build(controller);
            }
            else if (controller.ControllerType.ClosesType(typeof(SagaEndpointController<>), out Type[] sagaTypes))
            {
                if (_sagaActionBuilders.TryGetValue(sagaTypes[0], out var builder))
                    builder.Build(controller);
            }
        }
    }
}
