namespace MassTransitBenchmark.Latency
{
    using System;
    using System.Threading.Tasks;
    using Commands;
    using MassTransit;


    public class AmazonSqsMessageLatencyTransport :
        IMessageLatencyTransport
    {
        readonly AmazonSqsHostSettings _hostSettings;
        readonly IMessageLatencySettings _settings;
        IBusControl _busControl;
        Uri _targetAddress;
        ISendEndpoint _targetEndpoint;

        public AmazonSqsMessageLatencyTransport(AmazonSqsHostSettings hostSettings, IMessageLatencySettings settings)
        {
            _hostSettings = hostSettings;
            _settings = settings;
        }

        public async Task Send(Guid messageId, string payload)
        {
            await _targetEndpoint.Send(new LatencyTestMessage(messageId, payload)).ConfigureAwait(false);
        }

        public async Task Start(Action<IReceiveEndpointConfigurator> callback, IReportConsumerMetric messageMetricCapture)
        {
            _busControl = Bus.Factory.CreateUsingAmazonSqs(x =>
            {
                x.Host(_hostSettings);

                x.ReceiveEndpoint("latency_consumer" + (_settings.Durable ? "" : "_express"), e =>
                {
                    e.Durable = _settings.Durable;
                    e.PrefetchCount = _settings.PrefetchCount;

                    if (_settings.ConcurrencyLimit > 0)
                        e.ConcurrentMessageLimit = _settings.ConcurrencyLimit;

                    callback(e);

                    _targetAddress = e.InputAddress;
                });
            });

            await _busControl.StartAsync();

            _targetEndpoint = await _busControl.GetSendEndpoint(_targetAddress);
        }

        public async ValueTask DisposeAsync()
        {
            await _busControl.StopAsync();
        }
    }
}
