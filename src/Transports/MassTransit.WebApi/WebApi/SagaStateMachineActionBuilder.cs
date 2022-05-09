namespace MassTransit.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;


    public class SagaStateMachineActionBuilder<TInstance> :
        ISagaActionBuilder
        where TInstance : class, SagaStateMachineInstance
    {
        readonly IEndpointNameFormatter _endpointNameFormatter;
        readonly ISagaMessageActionBuilder _getActionBuilder;
        readonly IReadOnlyList<ISagaMessageActionBuilder> _messageActionBuilders;

        public SagaStateMachineActionBuilder(SagaStateMachine<TInstance> stateMachine, IEndpointNameFormatter endpointNameFormatter)
        {
            _endpointNameFormatter = endpointNameFormatter;
            _messageActionBuilders = StateMachineEvents(stateMachine).Select(type => CreateBuilder(type)).ToList();
            _getActionBuilder = new SagaStateMachineGetActionBuilder<TInstance>(endpointNameFormatter);
        }

        public Type SagaType => typeof(TInstance);

        public void Build(ControllerModel controller)
        {
            // use SagaDefinition to get endpoint name
            controller.ControllerName = _endpointNameFormatter.Saga<TInstance>();

            _getActionBuilder.Build(controller);

            foreach (var actionBuilder in _messageActionBuilders)
                actionBuilder.Build(controller);
        }

        ISagaMessageActionBuilder CreateBuilder(Type messageType)
        {
            var builderType = typeof(SagaStateMachineMessageActionBuilder<,>).MakeGenericType(typeof(TInstance), messageType);

            return Activator.CreateInstance(builderType, _endpointNameFormatter) as ISagaMessageActionBuilder
                ?? throw new InvalidOperationException("Failed to create Saga State Machine Action Builder");
        }

        static IEnumerable<Type> StateMachineEvents(SagaStateMachine<TInstance> stateMachine)
        {
            EventCorrelation[] correlations = stateMachine.Correlations.ToArray();

            correlations.SelectMany(x => x.Validate()).ThrowIfContainsFailure("Failed to compile state machine events");

            foreach (var correlation in correlations)
            {
                if (correlation.DataType.GetTypeInfo().IsValueType)
                    continue;

                yield return correlation.DataType;
            }
        }
    }
}
