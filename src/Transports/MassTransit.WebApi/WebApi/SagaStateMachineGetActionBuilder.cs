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


    public class SagaStateMachineGetActionBuilder<TInstance> :
        ISagaMessageActionBuilder
        where TInstance : class, SagaStateMachineInstance
    {
        readonly IEndpointNameFormatter _endpointNameFormatter;
        readonly MethodInfo _methodInfo;
        readonly List<IParameterModelBuilder> _parameterBuilders;

        public SagaStateMachineGetActionBuilder(IEndpointNameFormatter endpointNameFormatter)
        {
            _endpointNameFormatter = endpointNameFormatter;
            _methodInfo = typeof(SagaEndpointController<TInstance>).GetMethod(nameof(SagaEndpointController<TInstance>.Get))
                          ?? throw new InvalidOperationException("Could not get MethodInfo for message type");

            _parameterBuilders = _methodInfo.GetParameters().Select(parameter => (IParameterModelBuilder) Activator.CreateInstance(
                typeof(ParameterModelBuilder<>).MakeGenericType(parameter.ParameterType), parameter)).ToList();
        }

        public void Build(ControllerModel controller)
        {
            var instanceName = _endpointNameFormatter.Saga<TInstance>();

            var actionModel = new ActionModel(_methodInfo, typeof(TInstance).GetCustomAttributes().ToList())
            {
                Controller = controller,
                ActionName = $"Get{instanceName}",
            };

            foreach (var builder in _parameterBuilders) actionModel.Parameters.Add(builder.Create(actionModel));

            var selector = new SelectorModel();
            selector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] {HttpMethods.Get}));
            selector.EndpointMetadata.Add(new HttpMethodMetadata(new[] {HttpMethods.Get}));

            actionModel.Selectors.Add(selector);

            controller.Actions.Add(actionModel);
        }
    }
}
