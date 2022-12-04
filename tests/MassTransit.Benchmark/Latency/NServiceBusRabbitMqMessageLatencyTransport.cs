namespace MassTransitBenchmark.Latency
{
    using System;
    using System.Threading.Tasks;
    using Commands;
    using MassTransit;
    using NServiceBus;


    class NServiceBusRabbitMqMessageLatencyTransport :
        IMessageLatencyTransport
    {
        readonly RabbitMqHostSettings _hostSettings;
        readonly IMessageLatencySettings _settings;
        IEndpointInstance _endpointInstance;

        public NServiceBusRabbitMqMessageLatencyTransport(RabbitMqOptionSet hostSettings, IMessageLatencySettings settings)
        {
            _hostSettings = hostSettings;
            _settings = settings;
        }

        public async Task Send(Guid messageId, string payload)
        {
            await _endpointInstance.Send(new LatencyTestMessage(messageId, payload)).ConfigureAwait(false);
        }

        public async Task Start(Action<IReceiveEndpointConfigurator> callback, IReportConsumerMetric messageMetricCapture)
        {
            var endpointName = "latency_consumer_nsb" + (_settings.Durable ? "" : "_express");

            var endpointConfiguration = new EndpointConfiguration(endpointName);

            endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
            endpointConfiguration.EnableInstallers();

            endpointConfiguration.PurgeOnStartup(true);

            endpointConfiguration.LimitMessageProcessingConcurrencyTo(_settings.ConcurrencyLimit > 0 ? _settings.ConcurrencyLimit : 16);

            endpointConfiguration.Conventions()
                .DefiningCommandsAs(t => t.Namespace != null && t.Namespace.StartsWith("MassTransitBenchmark.Latency.Commands"));

            endpointConfiguration.RegisterComponents(
                configureComponents =>
                {
                    configureComponents.ConfigureComponent(() => messageMetricCapture, DependencyLifecycle.SingleInstance);
                    configureComponents.ConfigureComponent<LatencyTestMessageHandler>(DependencyLifecycle.InstancePerCall);
                });

            TransportExtensions<RabbitMQTransport> transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(
                $"host={_hostSettings.Host};username={_hostSettings.Username};password={_hostSettings.Password};virtualhost={_hostSettings.VirtualHost}");
            transport.PrefetchCount(_settings.PrefetchCount);

            if (!_settings.Durable)
                transport.DisableDurableExchangesAndQueues();

            transport.UseConventionalRoutingTopology(QueueType.Classic);

            RoutingSettings<RabbitMQTransport> routing = transport.Routing();
            routing.RouteToEndpoint(typeof(LatencyTestMessage), endpointName);

            _endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}
