namespace MassTransit.WebApi
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;


    public class MassTransitFeatureProvider :
        IApplicationFeatureProvider<ControllerFeature>
    {
        readonly IEnumerable<TypeInfo> _controllers;

        public MassTransitFeatureProvider(IEnumerable<IConsumerRegistration> consumers, IEnumerable<ISagaRegistration> sagas)
        {
            _controllers = consumers.Select(x => typeof(ConsumerEndpointController<>).MakeGenericType(x.Type).GetTypeInfo())
                .Concat(sagas.Select(x => typeof(SagaEndpointController<>).MakeGenericType(x.Type).GetTypeInfo()));
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var controller in _controllers)
                feature.Controllers.Add(controller);
        }
    }
}
