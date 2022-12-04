namespace MassTransitBenchmark.Latency
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;


    class RabbitMqMessageLatencyTransport :
        IMessageLatencyTransport
    {
        readonly RabbitMqHostSettings _hostSettings;
        readonly IMessageLatencySettings _settings;
        readonly bool _split;
        IBusControl _busControl;
        IBusControl _outboundBus;
        Uri _targetAddress;
        ISendEndpoint _targetEndpoint;
        ServiceProvider _provider;

        public RabbitMqMessageLatencyTransport(RabbitMqOptionSet hostSettings, IMessageLatencySettings settings)
        {
            _hostSettings = hostSettings;
            _settings = settings;

            _split = hostSettings.Split;
        }

        public async Task Send(Guid messageId, string payload)
        {
            await _targetEndpoint.Send(new LatencyTestMessage(messageId, payload)).ConfigureAwait(false);
        }

        public async Task Start(Action<IReceiveEndpointConfigurator> callback, IReportConsumerMetric messageMetricCapture)
        {
            _provider = new ServiceCollection()
                .AddMassTransit(x =>
                {
                    x.AddConsumer<MessageLatencyConsumer>();
                    x.AddSingleton(messageMetricCapture);

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(_hostSettings);

                        cfg.ReceiveEndpoint("latency_consumer" + (_settings.Durable ? "" : "_express"), e =>
                        {
                            e.PurgeOnStartup = true;
                            e.Durable = _settings.Durable;
                            e.PrefetchCount = _settings.PrefetchCount;

                            if (_settings.ConcurrencyLimit > 0)
                                e.ConcurrentMessageLimit = _settings.ConcurrencyLimit;

                            e.ConfigureConsumer<MessageLatencyConsumer>(context);

                            _targetAddress = e.InputAddress;
                        });
                    });
                })
                .BuildServiceProvider(true);

            _busControl = _provider.GetRequiredService<IBusControl>();

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await _busControl.StartAsync(timeout.Token);

            if (_split)
            {
                _outboundBus = Bus.Factory.CreateUsingRabbitMq(x =>
                {
                    x.Host(_hostSettings);

                    x.PrefetchCount = _settings.PrefetchCount;
                });

                await _outboundBus.StartAsync(timeout.Token);

                _targetEndpoint = await _outboundBus.GetSendEndpoint(_targetAddress);
            }
            else
                _targetEndpoint = await _busControl.GetSendEndpoint(_targetAddress);
        }

        public async ValueTask DisposeAsync()
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await _busControl.StopAsync(timeout.Token);

            if (_outboundBus != null)
                await _outboundBus.StopAsync(timeout.Token);
        }
    }
}
