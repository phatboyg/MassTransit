namespace MassTransitBenchmark.Latency
{
    using System;
    using System.Threading.Tasks;
    using Commands;
    using MassTransit;
    using MassTransit.Mediator;


    public class MediatorMessageLatencyTransport :
        IMessageLatencyTransport
    {
        readonly IMessageLatencySettings _settings;
        IMediator _mediator;

        public MediatorMessageLatencyTransport(IMessageLatencySettings settings)
        {
            _settings = settings;
        }

        public async Task Send(Guid messageId, string payload)
        {
            await _mediator.Send(new LatencyTestMessage(messageId, payload)).ConfigureAwait(false);
        }

        public Task Start(Action<IReceiveEndpointConfigurator> callback, IReportConsumerMetric messageMetricCapture)
        {
            _mediator = Bus.Factory.CreateMediator(callback);

            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return _mediator switch
            {
                IAsyncDisposable disposable => disposable.DisposeAsync(),
                _ => default
            };
        }
    }
}
