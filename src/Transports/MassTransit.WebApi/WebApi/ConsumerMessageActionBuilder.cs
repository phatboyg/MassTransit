namespace MassTransit.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;


    public class ConsumerMessageActionBuilder<TConsumer, TMessage> :
        IConsumerMessageActionBuilder
        where TConsumer : class
        where TMessage : class
    {
        readonly MethodInfo _consumerMethodInfo;
        readonly IEndpointNameFormatter _endpointNameFormatter;
        readonly MethodInfo _methodInfo;
        readonly List<IParameterModelBuilder> _parameterBuilders;

        public ConsumerMessageActionBuilder(IEndpointNameFormatter endpointNameFormatter)
        {
            _endpointNameFormatter = endpointNameFormatter;

            _consumerMethodInfo = typeof(TConsumer).GetMethod(nameof(IConsumer<TMessage>.Consume), new[] { typeof(ConsumeContext<TMessage>) })
                ?? throw new InvalidOperationException("Consume method not found");

            var produceType = _consumerMethodInfo.GetCustomAttribute<ProducesResponseTypeAttribute>()?.Type;

            //TODO: use GET if attr is found??


            _methodInfo = (produceType == null
                    ? typeof(ConsumerEndpointController<TConsumer>)
                        .GetMethod(nameof(ConsumerEndpointController<TConsumer>.Put))?
                        .MakeGenericMethod(typeof(TMessage))
                    : typeof(ConsumerEndpointController<TConsumer>)
                        .GetMethod(nameof(ConsumerEndpointController<TConsumer>.Post))?
                        .MakeGenericMethod(typeof(TMessage), produceType))
                ?? throw new InvalidOperationException("Could not get MethodInfo for message type");

            _parameterBuilders = _methodInfo.GetParameters().Select(parameter =>
            {
                var builderType = typeof(ParameterModelBuilder<>).MakeGenericType(parameter.ParameterType);
                return Activator.CreateInstance(builderType, parameter) as IParameterModelBuilder
                    ?? throw new InvalidOperationException("Failed to create parameter model builder");
            }).ToList();
        }

        public void Build(ControllerModel controller)
        {
            var actionName = _endpointNameFormatter.Message<TMessage>();
            HttpMethodAttribute[] methods = _consumerMethodInfo.GetCustomAttributes<HttpMethodAttribute>().ToArray();
            if (!methods.Any())
                methods = new HttpMethodAttribute[] { new HttpPutAttribute(actionName) };

            List<Attribute> attributes = typeof(TConsumer).GetCustomAttributes()
                .Union(_methodInfo.GetCustomAttributes())
                .ToList();

            var actionModel = new ActionModel(_methodInfo, attributes)
            {
                Controller = controller,
                ActionName = actionName
            };

            foreach (var builder in _parameterBuilders)
                actionModel.Parameters.Add(builder.Create(actionModel));

            var httpMethods = new HashSet<string>(methods.SelectMany(x => x.HttpMethods));
            var selector = new SelectorModel();
            selector.ActionConstraints.Add(new HttpMethodActionConstraint(httpMethods));
            selector.EndpointMetadata.Add(new HttpMethodMetadata(httpMethods));

            selector.AttributeRouteModel = methods.Aggregate(new AttributeRouteModel(),
                (current, next) => AttributeRouteModel.CombineAttributeRouteModel(current, new AttributeRouteModel { Template = next.Template })!);
            actionModel.Selectors.Add(selector);

            controller.Actions.Add(actionModel);
        }
    }
}
