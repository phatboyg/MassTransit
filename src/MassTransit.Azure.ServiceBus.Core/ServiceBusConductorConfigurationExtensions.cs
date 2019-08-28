namespace MassTransit
{
    using System;
    using Conductor.Configuration;
    using Conductor.Configuration.Configurators;
    using Conductor.Server;
    using Azure.ServiceBus.Core;
    using Azure.ServiceBus.Core.Configurators;


    public static class ServiceBusConductorConfigurationExtensions
    {
        /// <summary>
        /// Configure a service instance, which supports one or more receive endpoints, all of which are managed by conductor.
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="host"></param>
        /// <param name="configure"></param>
        public static void ServiceInstance(this IServiceBusBusFactoryConfigurator configurator, IServiceBusHost host,
            Action<IServiceInstanceConfigurator<IServiceBusReceiveEndpointConfigurator>> configure)
        {
            var instanceId = NewId.Next();
            var instanceEndpointName = ServiceEndpointNameFormatter.Instance.EndpointName(instanceId);

            configurator.ReceiveEndpoint(host, instanceEndpointName, endpointConfigurator =>
            {
                var instance = new ServiceInstance(instanceId, endpointConfigurator);

                var instanceConfigurator = new ServiceBusServiceInstanceConfigurator(configurator, host, instance);

                instanceConfigurator.ConfigureInstanceEndpoint(endpointConfigurator);

                configure?.Invoke(instanceConfigurator);
            });
        }
    }
}
