namespace MassTransit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using WebApi;


    public class MassTransitWebApiConfigureOptions :
        IConfigureOptions<MvcOptions>
    {
        readonly MassTransitFeatureProvider _featureProvider;
        readonly ApplicationPartManager _partManager;
        readonly EndpointControllerRouteConvention _routeConvention;

        public MassTransitWebApiConfigureOptions(ApplicationPartManager partManager, IServiceProvider provider)
        {
            _partManager = partManager;

            List<IConsumerRegistration> consumers = provider.GetServices<IConsumerRegistration>().ToList();
            List<ISagaRegistration> sagas = provider.GetServices<ISagaRegistration>().ToList();

            var formatter = provider.GetService<IEndpointNameFormatter>() ?? DefaultEndpointNameFormatter.Instance;

            _featureProvider = new MassTransitFeatureProvider(consumers, sagas);
            _routeConvention = new EndpointControllerRouteConvention(consumers, sagas, formatter, provider);
        }

        public void Configure(MvcOptions options)
        {
            _partManager.FeatureProviders.Add(_featureProvider);
            options.Conventions.Add(_routeConvention);
        }
    }
}
