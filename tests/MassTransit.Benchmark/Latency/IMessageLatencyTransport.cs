namespace MassTransitBenchmark.Latency
{
    using System;
    using System.Threading.Tasks;
    using MassTransit;


    public interface IMessageLatencyTransport :
        IAsyncDisposable
    {
        /// <summary>
        /// The bus control
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="messageMetricCapture"></param>
        Task Start(Action<IReceiveEndpointConfigurator> callback, IReportConsumerMetric messageMetricCapture);

        Task Send(Guid messageId, string payload);
    }
}
