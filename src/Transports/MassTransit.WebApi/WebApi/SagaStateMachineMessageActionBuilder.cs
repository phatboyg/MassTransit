namespace MassTransit.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Routing;


    public class SagaStateMachineMessageActionBuilder<TInstance, TMessage> :
        ISagaMessageActionBuilder
        where TInstance : class, SagaStateMachineInstance
        where TMessage : class
    {
        readonly IEndpointNameFormatter _endpointNameFormatter;
        readonly MethodInfo _methodInfo;
        readonly List<IParameterModelBuilder> _parameterBuilders;

        public SagaStateMachineMessageActionBuilder(IEndpointNameFormatter endpointNameFormatter)
        {
            _endpointNameFormatter = endpointNameFormatter;
            _methodInfo = typeof(SagaEndpointController<TInstance>)
                    .GetMethod(nameof(SagaEndpointController<TInstance>.Post))?.MakeGenericMethod(typeof(TMessage))
                ?? throw new InvalidOperationException("Could not get MethodInfo for message type");

            _parameterBuilders = _methodInfo.GetParameters().Select(parameter => (IParameterModelBuilder)Activator.CreateInstance(
                typeof(ParameterModelBuilder<>).MakeGenericType
                    (parameter.ParameterType), parameter)).ToList();
        }

        public void Build(ControllerModel controller)
        {
            var messageName = _endpointNameFormatter.Message<TMessage>();

            var actionModel = new ActionModel(_methodInfo, typeof(TInstance).GetCustomAttributes().ToList())
            {
                Controller = controller,
                ActionName = $"Post{messageName}",
            };

            foreach (var builder in _parameterBuilders)
                actionModel.Parameters.Add(builder.Create(actionModel));

            var selector = new SelectorModel();
            selector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { HttpMethods.Post }));
            selector.EndpointMetadata.Add(new HttpMethodMetadata(new[] { HttpMethods.Post }));

            selector.AttributeRouteModel = new AttributeRouteModel { Template = messageName };
            actionModel.Selectors.Add(selector);

            controller.Actions.Add(actionModel);
        }
    }
}
