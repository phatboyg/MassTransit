namespace MassTransitBenchmark.Latency
{
    using System;
    using System.Threading.Tasks;
    using Commands;
    using MassTransit;


    class InMemoryMessageLatencyTransport :
        IMessageLatencyTransport
    {
        readonly InMemoryOptionSet _optionSet;
        readonly IMessageLatencySettings _settings;
        IBusControl _busControl;
        Uri _targetAddress;
        ISendEndpoint _targetEndpoint;

        public InMemoryMessageLatencyTransport(InMemoryOptionSet optionSet, IMessageLatencySettings settings)
        {
            _optionSet = optionSet;
            _settings = settings;
        }

        public async Task Send(Guid messageId, string payload)
        {
            await _targetEndpoint.Send(new LatencyTestMessage(messageId, payload)).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _busControl.StopAsync();
        }

        public async Task Start(Action<IReceiveEndpointConfigurator> callback, IReportConsumerMetric messageMetricCapture)
        {
            _busControl = Bus.Factory.CreateUsingInMemory(x =>
            {
                x.ConcurrentMessageLimit = _optionSet.TransportConcurrencyLimit;

                x.ReceiveEndpoint("latency_consumer", e =>
                {
                    callback(e);
                    _targetAddress = e.InputAddress;
                });
            });

            await _busControl.StartAsync();

            _targetEndpoint = await _busControl.GetSendEndpoint(_targetAddress);
        }
    }
}
